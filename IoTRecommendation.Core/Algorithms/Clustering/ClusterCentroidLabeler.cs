using IoTRecommendation.Core.Models.Clustering;

namespace IoTRecommendation.Core.Algorithms.Clustering;

/// <summary>
/// Post-hoc interpretation of K-Means cluster centroids (display labels only).
/// Rules are applied after clustering; they do not assign cluster membership.
/// </summary>
public static class ClusterCentroidLabeler
{
    public const string ExtendedRangeLowPowerLabel = "Extended-Range Low-Power (Star/FAN/Mesh)";
    public const string ShortRangePanLabel = "Short-Range Ultra-Low-Power PAN";
    /// <summary>Assigns a descriptive label to each cluster from its centroid, then resolves duplicate labels.</summary>
    public static void ApplyLabels(IList<ClusterInfo> clusters)
    {
        foreach (var cluster in clusters)
            cluster.Label = InferPrimaryLabel(cluster.CentroidValues);

        ResolveDuplicateLabels(clusters);
    }

    /// <summary>
    /// Priority-ordered interpretation rules on centroid values (original units).
    /// </summary>
    public static string InferPrimaryLabel(IReadOnlyDictionary<string, double> centroid)
    {
        double dr = Get(centroid, "DataRate");
        double range = Get(centroid, "TransmissionRange");
        double energy = Get(centroid, "EnergyConsumption");
        bool hasRtt = centroid.ContainsKey("RTTLatency");
        double rtt = hasRtt ? centroid["RTTLatency"] : 0;
        double opex = Get(centroid, "AnnualConnectivityOPEX");
        bool hasOpex = centroid.ContainsKey("AnnualConnectivityOPEX");

        const double wlanRangeMaxM = 5000;
        const double wlanDataRateMbps = 100;
        const double wlanThroughputMbps = 1000;
        const double wlanEnergyMw = 1000;
        const double lpwanRangeMinM = 5000;
        const double lpwanDataRateMaxMbps = 10;
        const double lpwanLatencyMs = 1000;
        const double panEnergyMaxMw = 150;
        const double shortRangeMaxM = 500;
        const double meshRangeMinM = 500;
        const double cellularEnergyMinMw = 150;
        const double cellularDataRateMaxMbps = 1000;
        const double cellularDataRateMinMbps = 0.5;
        const double cellularOpexMin = 3.5;

        bool shortRange = range < wlanRangeMaxM;
        bool wlanThroughput =
            shortRange &&
            (dr >= wlanThroughputMbps ||
             (dr >= wlanDataRateMbps && energy >= wlanEnergyMw));

        if (wlanThroughput)
            return "High-Throughput WLAN";

        bool longRange = range >= lpwanRangeMinM;
        bool lpwanLikeRate = dr < lpwanDataRateMaxMbps;
        bool lpwanLikeLatency = hasRtt && rtt > lpwanLatencyMs;
        if (longRange && (lpwanLikeRate || lpwanLikeLatency))
            return "Long-Range LPWAN";

        bool cellularLike =
            shortRange &&
            energy >= cellularEnergyMinMw &&
            dr >= cellularDataRateMinMbps &&
            dr < cellularDataRateMaxMbps &&
            (!hasOpex || opex >= cellularOpexMin);

        if (cellularLike)
            return "Cellular IoT (LTE-M / NR-Light)";

        if (energy < panEnergyMaxMw && range >= meshRangeMinM && shortRange)
            return ExtendedRangeLowPowerLabel;

        if (energy < panEnergyMaxMw && range < shortRangeMaxM)
            return ShortRangePanLabel;

        if (longRange)
            return "Long-Range Wide-Area";

        if (dr >= wlanDataRateMbps && shortRange)
            return "High-Throughput Short-Range";

        if (energy >= cellularEnergyMinMw && shortRange)
            return "Cellular IoT (LTE-M / NR-Light)";

        return "Moderate-Range IoT Connectivity";
    }

    private static void ResolveDuplicateLabels(IList<ClusterInfo> clusters)
    {
        var groups = clusters.GroupBy(c => c.Label).Where(g => g.Count() > 1);
        foreach (var group in groups)
        {
            if (group.Key is ShortRangePanLabel or "Ultra-Low-Power PAN/Mesh")
            {
                RelabelByRange(group, ShortRangePanLabel, ExtendedRangeLowPowerLabel, 500);
                continue;
            }

            if (group.Key is ExtendedRangeLowPowerLabel or "Long-Range Low-Power Mesh")
            {
                RelabelByRange(group, ShortRangePanLabel, ExtendedRangeLowPowerLabel, 500);
                continue;
            }

            var ordered = group.OrderBy(c => Get(c.CentroidValues, "TransmissionRange")).ToList();
            for (int i = 0; i < ordered.Count; i++)
            {
                double range = Get(ordered[i].CentroidValues, "TransmissionRange");
                ordered[i].Label = $"{group.Key} (range ≈ {FormatRange(range)})";
            }
        }
    }

    private static void RelabelByRange(
        IEnumerable<ClusterInfo> group,
        string shortLabel,
        string longLabel,
        double splitM)
    {
        foreach (var cluster in group)
        {
            double range = Get(cluster.CentroidValues, "TransmissionRange");
            cluster.Label = range >= splitM ? longLabel : shortLabel;
        }
    }

    private static double Get(IReadOnlyDictionary<string, double> centroid, string key) =>
        centroid.TryGetValue(key, out double v) ? v : 0;

    private static string FormatRange(double meters) =>
        meters >= 1000 ? $"{meters / 1000:F1} km" : $"{meters:F0} m";
}
