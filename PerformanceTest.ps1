# Performance Test Script for Article API Optimization
# Run this in PowerShell to compare original vs optimized endpoints

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Article API Performance Comparison Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$baseUrl = "https://localhost:5001" # Update with your actual URL
$company = "Derendinger-Switzerland" # Update with your actual company name

# Test payload
$testPayload = @{
    customerNr = "12345"
    sendMethod = "Delivery"
    partialDelivery = $true
    deliveryAddressId = "ADDR001"
    pickupBranchId = "BRANCH001"
    pickingWarehouse = "WH001"
    isTourTimetable = $false
    items = @(
        @{ articleId = "ART001"; quantity = 1.0 },
        @{ articleId = "ART002"; quantity = 2.0 },
        @{ articleId = "ART003"; quantity = 1.0 },
        @{ articleId = "ART004"; quantity = 3.0 },
        @{ articleId = "ART005"; quantity = 1.0 }
    )
} | ConvertTo-Json

# Number of test iterations
$iterations = 10

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Base URL: $baseUrl"
Write-Host "  Company: $company"
Write-Host "  Test Items: 5 articles"
Write-Host "  Iterations: $iterations"
Write-Host ""

# Ignore SSL certificate errors (for development only)
if (-not ([System.Management.Automation.PSTypeName]'ServerCertificateValidationCallback').Type) {
    $certCallback = @"
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    public class ServerCertificateValidationCallback {
        public static void Ignore() {
            if(ServicePointManager.ServerCertificateValidationCallback == null) {
                ServicePointManager.ServerCertificateValidationCallback += 
                    delegate (
                        Object obj, 
                        X509Certificate certificate, 
                        X509Chain chain, 
                        SslPolicyErrors errors
                    ) {
                        return true;
                    };
            }
        }
    }
"@
    Add-Type $certCallback
}
[ServerCertificateValidationCallback]::Ignore()

# Function to measure endpoint performance
function Test-Endpoint {
    param(
        [string]$EndpointUrl,
        [string]$EndpointName,
        [string]$Payload
    )
    
    Write-Host "Testing: $EndpointName" -ForegroundColor Green
    $times = @()
    
    for ($i = 1; $i -le $iterations; $i++) {
        Write-Progress -Activity "Testing $EndpointName" -Status "Iteration $i of $iterations" -PercentComplete (($i / $iterations) * 100)
        
        try {
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            
            $response = Invoke-RestMethod `
                -Uri $EndpointUrl `
                -Method POST `
                -Body $Payload `
                -ContentType "application/json" `
                -ErrorAction Stop
            
            $stopwatch.Stop()
            $times += $stopwatch.ElapsedMilliseconds
            
            Write-Host "  Iteration $i`: $($stopwatch.ElapsedMilliseconds)ms" -ForegroundColor Gray
        }
        catch {
            Write-Host "  Iteration $i`: ERROR - $($_.Exception.Message)" -ForegroundColor Red
        }
        
        Start-Sleep -Milliseconds 100 # Small delay between requests
    }
    
    Write-Progress -Activity "Testing $EndpointName" -Completed
    
    if ($times.Count -gt 0) {
        $avg = [Math]::Round(($times | Measure-Object -Average).Average, 2)
        $min = ($times | Measure-Object -Minimum).Minimum
        $max = ($times | Measure-Object -Maximum).Maximum
        $median = ($times | Sort-Object)[[Math]::Floor($times.Count / 2)]
        
        return @{
            Average = $avg
            Min = $min
            Max = $max
            Median = $median
            Successful = $times.Count
        }
    }
    else {
        return $null
    }
}

# Test Original Endpoint
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TESTING ORIGINAL ENDPOINT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
$originalUrl = "$baseUrl/apps/prod-webshop-service-app/webshop-service/test-optimized/articles/$company/availabilities-original"
$originalResults = Test-Endpoint -EndpointUrl $originalUrl -EndpointName "Original" -Payload $testPayload

# Test Optimized Endpoint
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TESTING OPTIMIZED ENDPOINT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
$optimizedUrl = "$baseUrl/apps/prod-webshop-service-app/webshop-service/test-optimized/articles/$company/availabilities"
$optimizedResults = Test-Endpoint -EndpointUrl $optimizedUrl -EndpointName "Optimized" -Payload $testPayload

# Display Results
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PERFORMANCE COMPARISON RESULTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($originalResults -and $optimizedResults) {
    Write-Host "ORIGINAL ENDPOINT:" -ForegroundColor Yellow
    Write-Host "  Average:    $($originalResults.Average)ms"
    Write-Host "  Median:     $($originalResults.Median)ms"
    Write-Host "  Min:        $($originalResults.Min)ms"
    Write-Host "  Max:        $($originalResults.Max)ms"
    Write-Host "  Successful: $($originalResults.Successful)/$iterations"
    Write-Host ""
    
    Write-Host "OPTIMIZED ENDPOINT:" -ForegroundColor Green
    Write-Host "  Average:    $($optimizedResults.Average)ms"
    Write-Host "  Median:     $($optimizedResults.Median)ms"
    Write-Host "  Min:        $($optimizedResults.Min)ms"
    Write-Host "  Max:        $($optimizedResults.Max)ms"
    Write-Host "  Successful: $($optimizedResults.Successful)/$iterations"
    Write-Host ""
    
    $improvement = [Math]::Round((($originalResults.Average - $optimizedResults.Average) / $originalResults.Average) * 100, 2)
    $timeSaved = [Math]::Round($originalResults.Average - $optimizedResults.Average, 2)
    
    Write-Host "PERFORMANCE IMPROVEMENT:" -ForegroundColor Magenta
    Write-Host "  Time Saved:  ${timeSaved}ms per request"
    Write-Host "  Improvement: ${improvement}% faster"
    
    if ($improvement -gt 0) {
        Write-Host ""
        Write-Host "? Optimized version is FASTER!" -ForegroundColor Green
    }
    elseif ($improvement -lt 0) {
        Write-Host ""
        Write-Host "? Optimized version is SLOWER (investigate!)" -ForegroundColor Red
    }
    else {
        Write-Host ""
        Write-Host "? No significant difference" -ForegroundColor Yellow
    }
}
else {
    Write-Host "ERROR: Could not complete performance comparison" -ForegroundColor Red
    Write-Host "Please check that the API is running and the URLs are correct" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
