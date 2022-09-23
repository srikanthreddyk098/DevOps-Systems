using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AzureAutomation.Models;
using Newtonsoft.Json;

namespace AzureAutomation.Web
{
    public class AzureVmPriceService
    {
        private IEnumerable<AzureVmPrice> _azureVmEastUsPrices;
        private IEnumerable<AzureVmPrice> _azureVmEastUs2Prices;
        private IEnumerable<AzureVmPrice> _azureVmWestUsPrices;
        private IEnumerable<AzureVmPrice> _azureVmWestUs2Prices;
        private IEnumerable<AzureVmPrice> _azureVmCentralUsPrices;
        private IEnumerable<AzureVmPrice> _azureVmNorthCentralUsPrices;
        private IEnumerable<AzureVmPrice> _azureVmSouthCentralUsPrices;
        private IEnumerable<AzureVmPrice> _azureVmWestCentralUsPrices;

        private async Task<IEnumerable<AzureVmPrice>> GetEastUsPricesAsync()
        {
            return _azureVmEastUsPrices ??= await GetPricesForRegionAsync("eastus");
        }

        private async Task<IEnumerable<AzureVmPrice>> GetEastUs2PricesAsync()
        {
            return _azureVmEastUs2Prices ??= await GetPricesForRegionAsync("eastus2");
        }

        private async Task<IEnumerable<AzureVmPrice>> GetWestUsPricesAsync()
        {
            return _azureVmWestUsPrices ??= await GetPricesForRegionAsync("westus");
        }

        private async Task<IEnumerable<AzureVmPrice>> GetWestUs2PricesAsync()
        {
            return _azureVmWestUs2Prices ??= await GetPricesForRegionAsync("westus2");
        }

        private async Task<IEnumerable<AzureVmPrice>> GetCentralUsPricesAsync()
        {
            return _azureVmCentralUsPrices ??= await GetPricesForRegionAsync("centralus");
        }

        private async Task<IEnumerable<AzureVmPrice>> GetNorthCentralUsPricesAsync()
        {
            return _azureVmNorthCentralUsPrices ??= await GetPricesForRegionAsync("northcentralus");
        }

        private async Task<IEnumerable<AzureVmPrice>> GetSouthCentralUsPricesAsync()
        {
            return _azureVmSouthCentralUsPrices ??= await GetPricesForRegionAsync("southcentralus");
        }

        private async Task<IEnumerable<AzureVmPrice>> GetWestCentralUsPricesAsync()
        {
            return _azureVmWestCentralUsPrices ??= await GetPricesForRegionAsync("westcentralus");
        }

        private static async Task<IEnumerable<AzureVmPrice>> GetPricesForRegionAsync(string region)
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"https://azureprice.net/?currency=USD&timeoption=hour&region={region}");
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            string prices = responseBody.Split(new[] {"json = "}, StringSplitOptions.None)[1].Split(';')[0];

            return JsonConvert.DeserializeObject<IEnumerable<AzureVmPrice>>(prices);
        }

        internal async Task<double?> GetVmPriceAsync(string osType, string region, string vmSize)
        {
            switch (region) {
                case "eastus":
                {
                    var prices = await GetEastUsPricesAsync();
                    if (osType.Equals("Linux")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.LinuxPrice;
                    }

                    if (osType.Equals("Windows")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.WindowsPrice;
                    }

                    return null;
                }
                case "eastus2":
                {
                    var prices = await GetEastUs2PricesAsync();
                    if (osType.Equals("Linux")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.LinuxPrice;
                    }

                    if (osType.Equals("Windows")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.WindowsPrice;
                    }

                    return null;
                }
                case "westus":
                {
                    var prices = await GetWestUsPricesAsync();
                    if (osType.Equals("Linux")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.LinuxPrice;
                    }

                    if (osType.Equals("Windows")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.WindowsPrice;
                    }

                    return null;
                }
                case "westus2":
                {
                    var prices = await GetWestUs2PricesAsync();
                    if (osType.Equals("Linux")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.LinuxPrice;
                    }

                    if (osType.Equals("Windows")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.WindowsPrice;
                    }

                    return null;
                }
                case "centralus":
                {
                    var prices = await GetCentralUsPricesAsync();
                    if (osType.Equals("Linux")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.LinuxPrice;
                    }

                    if (osType.Equals("Windows")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.WindowsPrice;
                    }

                    return null;
                }
                case "northcentralus":
                {
                    var prices = await GetNorthCentralUsPricesAsync();
                    if (osType.Equals("Linux")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.LinuxPrice;
                    }

                    if (osType.Equals("Windows")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.WindowsPrice;
                    }

                    return null;
                }
                case "southcentralus":
                {
                    var prices = await GetSouthCentralUsPricesAsync();
                    if (osType.Equals("Linux")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.LinuxPrice;
                    }

                    if (osType.Equals("Windows")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.WindowsPrice;
                    }

                    return null;
                }
                case "westcentralus":
                {
                    var prices = await GetWestCentralUsPricesAsync();
                    if (osType.Equals("Linux")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.LinuxPrice;
                    }

                    if (osType.Equals("Windows")) {
                        return prices.FirstOrDefault(x => x.RegionId.Equals(region, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(vmSize, StringComparison.OrdinalIgnoreCase))?.WindowsPrice;
                    }

                    return null;
                }
                default:
                {
                    return null;
                }
            }
        }
    }
}