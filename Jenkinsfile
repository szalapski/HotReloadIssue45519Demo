pipeline {
    agent {
        kubernetes {
        yaml '''
apiVersion: v1
kind: Pod
spec:
    containers:
        -
            name: argo
            image: 'docker.generalmills.com/k8s-argocli:stable'
            command:
                - cat
            tty: true
'''
        }
    }
    environment {
        PROJECT_NAME = 'trade-cars'
        IMAGE_REGISTRY = 'docker.generalmills.com'
        GIT_COMMIT_SHORT = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
        CLEAN_BRANCH_NAME = "${env.BRANCH_NAME.toLowerCase().replaceAll(/[^a-z0-9-]+/,'-')}"
    }

    stages {
        stage('Setup') {
            steps {
                beginBuild()
                updatePullRequestStatus state: 'pending'
            }
        }

        stage('Build') {
            agent { label 'linux' }
            steps {
                script {
                    env.BUILD_TAG = "$BUILD_NUMBER-$GIT_COMMIT_SHORT"
                    env.ORIGINAL_BUILD_NUMBER = "$BUILD_NUMBER"
                }

                sh 'docker build . -t $IMAGE_REGISTRY/docker-snapshot-local/$PROJECT_NAME:$BUILD_TAG --build-arg branch_build_commit="$CLEAN_BRANCH_NAME $BUILD_NUMBER $GIT_COMMIT_SHORT"'
                pushToArtifactoryContainerImage image: "$PROJECT_NAME", tag: "$BUILD_TAG", registry: "$IMAGE_REGISTRY"
                scanBuild buildName: "$PROJECT_NAME", buildNumber: "$BUILD_TAG"
            }
        }

        stage('Deploy dev/alpha/stable') {
            environment {
                // master goes to "dev", a release branch to "stable", the rest to "alpha"
                HOST_ENVIRONMENT = "${CLEAN_BRANCH_NAME == "master" ? "development" : CLEAN_BRANCH_NAME ==~ /release-.*/ ? "stable" : "alpha"}"
		        SUBDOMAIN_SUFFIX = "${HOST_ENVIRONMENT == "development" ? "dev" : HOST_ENVIRONMENT}"
                HOST = "${PROJECT_NAME}-${SUBDOMAIN_SUFFIX}.k8s.genmills.com"
            }
            steps {
                echo "Attempting deploy via Argo for a host at https://${HOST} with ASPNETCORE_ENVIRONMENT=${HOST_ENVIRONMENT}"
                container('argo') {
                    deployK8sApplicationV2 cluster_env: 'nonprod', app_env: env.SUBDOMAIN_SUFFIX, extra_args:[
                        helm: [
                            releaseName: env.PROJECT_NAME,
                            values: [
                                image: [
                                    "repository": "${IMAGE_REGISTRY}/${PROJECT_NAME}",
                                    "tag": "$BUILD_TAG",
                                ],
                                istio: [
                                    hosts: [env.HOST],
                                ],
                                extraEnvs: [ "ASPNETCORE_ENVIRONMENT": env.HOST_ENVIRONMENT ]
                            ],
                        ],
                    ]
                }
                echo "Site was deployed to a host at https://${HOST}"
            }
        }

       stage('Deploy to QA?') {
            agent none
	    when { anyOf { branch 'release-*'; branch 'master' } }
            steps {
                script {
                    safeInput message: 'Deploy to QA?',
                        timeout: 30,
                        timeoutUnit: 'MINUTES',
                        submitter: 'DEPLOY_QA_TRADEUS_PARAGON'
                }
            }
        }
        stage('Deploy QA') {
            when { anyOf { branch 'release-*'; branch 'master' }  }
		    environment {
                HOST_ENVIRONMENT = "qa"
		SUBDOMAIN_SUFFIX = "${HOST_ENVIRONMENT}"
                HOST = "${PROJECT_NAME}-${SUBDOMAIN_SUFFIX}.k8s.genmills.com"
            }
            steps {
                echo "Attempting deploy via Argo for a host at https://${HOST} with ASPNETCORE_ENVIRONMENT=${HOST_ENVIRONMENT}"
                container('argo') {
                    deployK8sApplicationV2 cluster_env: 'nonprod', app_env: env.SUBDOMAIN_SUFFIX, extra_args: [
                        helm: [
                            releaseName: env.PROJECT_NAME,
                            values: [
                                image: [
                                    "repository": "${IMAGE_REGISTRY}/${PROJECT_NAME}",
                                    "tag": "$BUILD_TAG",
                                ],
                                istio: [
                                    hosts: [env.HOST],
                                ],
                                extraEnvs: [ "ASPNETCORE_ENVIRONMENT": env.HOST_ENVIRONMENT ]
                            ],
                        ],
                    ]
                }
                echo "Site was deployed to a host at https://${HOST}"
            }
        }

        stage('Deploy to Prod?') {
            agent none
            when { anyOf { branch 'release-*'; branch 'master'  } }
            steps {
                script {
                    def deployApproval = safeInput message: 'Deploy to Production?',
                        ok: 'Deploy to P now',
                        cancel: 'End job now',
                        timeout: 60,
                        timeoutUnit: 'MINUTES'
                }
            }
        }


        stage('Deploy Production') {
            when { anyOf { branch 'release-*'; branch 'master'  } }
		    environment {
                HOST_ENVIRONMENT = "production"
		        SUBDOMAIN_SUFFIX = "prod"
                HOST = "tradecars.genmills.com"
            }
            steps {
                echo "Attempting deploy via Argo for a host at https://${HOST} with ASPNETCORE_ENVIRONMENT=${HOST_ENVIRONMENT}"
                container('argo') {
                    deployK8sApplicationV2 cluster_env: 'prod', app_env: env.SUBDOMAIN_SUFFIX, extra_args: [
                        helm: [
                            releaseName: env.PROJECT_NAME,
                            values: [
                                image: [
                                    "repository": "${IMAGE_REGISTRY}/${PROJECT_NAME}",
                                    "tag": "${BUILD_NUMBER}-${GIT_COMMIT_SHORT}",
                                ],
                                istio: [
                                    hosts: [env.HOST],
                                ],
                                extraEnvs: [ "ASPNETCORE_ENVIRONMENT": env.HOST_ENVIRONMENT ]
                            ],
                        ],
                    ]
                }
                echo "Site was deployed to a host at https://${HOST}"
            }
        }
    }

    post {
        always {
            postBuild()
        }
        success {
            updatePullRequestStatus state: 'succeeded'
        }
        failure {
            updatePullRequestStatus state: 'failed'
        }
    }
}
