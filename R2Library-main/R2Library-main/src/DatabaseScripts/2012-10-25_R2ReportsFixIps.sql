
update ContentView 
set    ipAddressInteger = (cast((cast(ipAddressOctetA as bigint) * 256 * 256 * 256) as bigint)
                     + cast((ipAddressOctetB * 256 * 256) as bigint) 
                     + cast((ipAddressOctetC * 256) as bigint) 
                     + cast(ipAddressOctetD as bigint))

update PageView
set    ipAddressInteger = (cast((cast(ipAddressOctetA as bigint) * 256 * 256 * 256) as bigint)
                     + cast((ipAddressOctetB * 256 * 256) as bigint) 
                     + cast((ipAddressOctetC * 256) as bigint) 
                     + cast(ipAddressOctetD as bigint))

update Search
set    ipAddressInteger = (cast((cast(ipAddressOctetA as bigint) * 256 * 256 * 256) as bigint)
                     + cast((ipAddressOctetB * 256 * 256) as bigint) 
                     + cast((ipAddressOctetC * 256) as bigint) 
                     + cast(ipAddressOctetD as bigint))


