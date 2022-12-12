# TradeCars

TradeCars is a ASP.NET Core site for users to edit the Corporate Accrual Rate Split percentages 
for customers like Costco.

To run in Linux in a docker container locally:
```
docker build --progress plain -t tradecars . && docker run -it --init --rm --env ASPNETCORE_ENVIRONMENT=Developer --env ASPNETCORE_URLS=https://*:8080 -p 7129:8080 -e ASPNETCORE_Kestrel__Certificates__Default__Path=/app/httpsCert.pfx -e ASPNETCORE_Kestrel__Certificates__Default__Password="" tradecars
```

- Alpha: https://trade-cars-alpha.k8s.genmills.com
- Dev (**master**): https://trade-cars-dev.k8s.genmills.com
- Stable: https://trade-cars-stable.k8s.genmills.com , if any
- QA: https://trade-cars-service-qa.k8s.genmills.com
- Production: https://tradecars.genmills.com

