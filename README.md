# Multi-Vehicle Storage Search API

This project implements a search algorithm for finding locations where renters can store multiple vehicles based on size and quantity.

Built as part of a technical assessment.

---

## ✨ Features

- Accepts vehicle storage requests via JSON POST
- Matches listings from a provided dataset
- Returns the cheapest valid combinations per location
- Optimized for fast response times (<300ms target)
- Deployed live to Azure App Service

---

## 🚀 Live API

**POST** [https://carstorage.azurewebsites.net/](https://carstorage.azurewebsites.net/)

Example request:

```bash
curl -X POST "https://https://carstorage.azurewebsites.net/" \
    -H "Content-Type: application/json" \
    -d '[
            { "length": 10, "quantity": 1 },
            { "length": 20, "quantity": 2 }
        ]'

```

## VehicleInquiryMatcherAllPermutations (Exploratory)

This class represents an early approach to solving the vehicle-to-listing matching problem using exhaustive permutation and recursive fitting strategies.

Although the final application uses a heuristic-driven greedy matcher for better performance, I chose to retain this code as a demonstration of:

- Deep exploration of the problem space
- Understanding of bin-packing and fitting algorithms
- Recognition of trade-offs between optimality and computational feasibility

It is isolated from production code to maintain project simplicity while highlighting algorithmic depth for reviewers interested in the engineering behind the solution.


## 🚀 Deployment Instructions

This project is deployed to Azure App Service using the following process:

### 1. Build the Project

Publish the WebAPI targeting Windows x64 for Azure App Service:

```bash
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

### 2. Package the Application

Zip the published output:

``` bash
cd publish
zip -r ../app.zip .
cd ..
```

### 3. Deploy to Azure App Service

Deploy the zipped package using Azure CLI:

``` bash
az webapp deploy --resource-group <your-resource-group> --name <your-webapp-name> --src-path app.zip
```

Ensure the App Service is configured to use a 64-bit worker process:

``` bash
az webapp config set --resource-group <your-resource-group> --name <your-webapp-name> --use-32bit-worker-process false
```
