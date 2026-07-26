using IoTRecommendation.Core.Algorithms.Clustering;
using Xunit;

namespace IoTRecommendation.Core.Tests;

public sealed class ClusterCentroidLabelerTests
{
    [Fact]
    public void UserReportedCentroids_GetDistinctMeaningfulLabels()
    {
        var pan = Centroid(dr: 1.8, range: 75, energy: 21, opex: 1);
        var cellular = Centroid(dr: 75, range: 2650, energy: 331, opex: 4.5);
        var lpwan = Centroid(dr: 0.1, range: 22333, energy: 74, rtt: 22333);
        var mesh = Centroid(dr: 0.8, range: 1200, energy: 24, opex: 2);
        var wlan = Centroid(dr: 16330, range: 32, energy: 20248, opex: 3);

        Assert.Equal("Short-Range Ultra-Low-Power PAN", ClusterCentroidLabeler.InferPrimaryLabel(pan));
        Assert.Equal("Cellular IoT (LTE-M / NR-Light)", ClusterCentroidLabeler.InferPrimaryLabel(cellular));
        Assert.Equal("Long-Range LPWAN", ClusterCentroidLabeler.InferPrimaryLabel(lpwan));
        Assert.Equal(ClusterCentroidLabeler.ExtendedRangeLowPowerLabel, ClusterCentroidLabeler.InferPrimaryLabel(mesh));
        Assert.Equal("High-Throughput WLAN", ClusterCentroidLabeler.InferPrimaryLabel(wlan));

        Assert.DoesNotContain("Mixed", ClusterCentroidLabeler.InferPrimaryLabel(pan));
    }

    [Fact]
    public void RedCapCentroid_IsNotMislabeledAsWlan()
    {
        var redCap = Centroid(dr: 150, range: 300, energy: 366, opex: 5);
        Assert.Equal("Cellular IoT (LTE-M / NR-Light)", ClusterCentroidLabeler.InferPrimaryLabel(redCap));
    }

    private static Dictionary<string, double> Centroid(
        double dr,
        double range,
        double energy,
        double opex = 0,
        double? rtt = null)
    {
        var d = new Dictionary<string, double>
        {
            ["DataRate"] = dr,
            ["TransmissionRange"] = range,
            ["EnergyConsumption"] = energy,
            ["AnnualConnectivityOPEX"] = opex
        };
        if (rtt.HasValue)
            d["RTTLatency"] = rtt.Value;
        return d;
    }
}
