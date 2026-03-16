# R2Library Local Development - Quick Start Guide

## One-Time Setup (Do This First!)

### Step 1: Create Local Directories

Run the PowerShell script to create all required directories:

```powershell
# Run from the repository root directory
.\setup-local-dev-directories.ps1
```

This creates:
- `C:\Temp\R2Library\Logs` - Application logs
- `C:\Temp\R2Library\MessageQueueErrors` - Failed message queue writes
- `C:\Temp\R2Library\Content\xml` - XML content storage
- `C:\Temp\R2Library\Content\cache` - Content cache
- `C:\Temp\R2Library\Content\R2HtmlIndex` - DTSearch index
- `C:\Temp\R2Library\_Static\Xsl` - XSL transformation files

### Step 2: Verify Configuration

Check that `R2V2.Web/User.config` contains:

```xml
<add key="Environment.IsLocalDevelopment" value="true" />
<add key="MessageQueue.Enabled" value="false" />
```

### Step 3: Clean and Rebuild

```
1. Clean Solution (Ctrl+Shift+B)
2. Rebuild Solution
```

## Running the Application

1. **Start Debugging** (F5)
2. **First load will take ~4-5 seconds** (resource cache loading)
3. **Subsequent pages load in <1 second**

## What You'll See

### In Debug Output Window

? **Good messages (expected):**
```
Message queue disabled in local dev - skipping queue 'Q.Stage.RequestData'
Request logging disabled in local dev - Request: /Browse, Duration: 245ms
Request completed: /Home/Index, Duration: 156ms
```

? **Bad messages (if you see these, something's wrong):**
```
System.TimeoutException in EasyNetQ.dll
DirectoryNotFoundException: Could not find 'D:\R2library\...'
Message sent to Q.Stage.RequestData in 34582 ms
```

## Performance Benchmarks

You should see these approximate timings:

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Application Startup | 19s | 4-5s | 74% faster |
| First Request | 40s | 2-3s | 93% faster |
| Subsequent Requests | 34s | <1s | 97% faster |

## Switching Back to Production Mode

When deploying or testing with real message queue:

1. Edit `R2V2.Web/User.config`:
   ```xml
   <add key="Environment.IsLocalDevelopment" value="false" />
   ```

2. Or comment out the entire line to use defaults

## Troubleshooting

### Issue: Pages still taking 30+ seconds

**Solution:** Check that `Environment.IsLocalDevelopment` is set to `true` in User.config

### Issue: DirectoryNotFoundException errors

**Solution:** Run `setup-local-dev-directories.ps1` again

### Issue: NullReferenceException in logging

**Solution:** Clear ASP.NET temporary files:
```powershell
Remove-Item -Recurse -Force "$env:LOCALAPPDATA\Temp\Temporary ASP.NET Files\*"
```

### Issue: Application won't start

**Solution:** 
1. Verify database connection string in `hibernate.config`
2. Check that local SQL Server is running
3. Review Visual Studio Output window for specific errors

## What Was Changed

Files modified for local development performance:

1. **R2V2.Web/User.config** - Local development settings
2. **R2V2/Infrastructure/MessageQueue/MessageQueueService.cs** - Skip RabbitMQ
3. **R2V2/Core/RequestLogger/RequestLoggerService.cs** - Skip request logging
4. **R2V2.Web/Infrastructure/MvcFramework/Filters/RequestLoggerFilter.cs** - Local dev check
5. **R2V2.Web/log4net.config** - Optimized logging configuration

## Next Steps

- ? Run the setup script
- ? Verify configuration
- ? Rebuild solution
- ? Start debugging
- ? Enjoy fast local development! ??

---

**Questions?** Review `LOCAL_DEV_PERFORMANCE_OPTIMIZATION.md` for detailed technical information.
