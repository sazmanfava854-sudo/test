import { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Avatar from '@mui/material/Avatar';
import Alert from '@mui/material/Alert';
import { useTheme } from '@mui/material/styles';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  PointElement,
  LineElement,
  ArcElement,
  RadialLinearScale,
  Tooltip,
  Legend,
  Filler,
} from 'chart.js';
import { Bar, Line, Pie, Radar } from 'react-chartjs-2';
import PeopleIcon from '@mui/icons-material/People';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import WarningIcon from '@mui/icons-material/Warning';
import CancelIcon from '@mui/icons-material/Cancel';
import StarIcon from '@mui/icons-material/Star';
import StatCard from '../../components/common/StatCard';
import ChartCard from '../../components/common/ChartCard';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import { dashboardService } from '../../services/dashboardService';
import type { ManagerDashboardDto } from '../../types';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  PointElement,
  LineElement,
  ArcElement,
  RadialLinearScale,
  Tooltip,
  Legend,
  Filler,
);

const emptyData: ManagerDashboardDto = {
  employeeCount: 0,
  todayPresent: 0,
  todayDelays: 0,
  todayAbsent: 0,
  averageScore: 0,
  topEmployees: [],
  weakEmployees: [],
  monthlyTrend: [],
  teamIndicators: [],
};

export default function ManagerDashboard() {
  const theme = useTheme();
  const [data, setData] = useState<ManagerDashboardDto>(emptyData);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await dashboardService.getManagerDashboard();
        if (response.success && response.data) {
          setData(response.data);
        } else {
          setError(response.message ?? 'خطا در دریافت داده‌های داشبورد');
        }
      } catch {
        setError('خطا در اتصال به سرور');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const labels = data.monthlyTrend.map((t) => t.label);
  const values = data.monthlyTrend.map((t) => t.value);

  const radarData = {
    labels: data.teamIndicators.map((i) => i.label),
    datasets: [
      {
        label: 'میانگین تیم',
        data: data.teamIndicators.map((i) => i.value),
        backgroundColor: `${theme.palette.primary.main}33`,
        borderColor: theme.palette.primary.main,
        pointBackgroundColor: theme.palette.secondary.main,
      },
    ],
  };

  const barData = {
    labels,
    datasets: [
      {
        label: 'میانگین امتیاز',
        data: values,
        backgroundColor: `${theme.palette.primary.main}99`,
        borderRadius: 6,
      },
    ],
  };

  const lineData = {
    labels,
    datasets: [
      {
        label: 'روند ماهانه',
        data: values,
        borderColor: theme.palette.secondary.main,
        backgroundColor: `${theme.palette.secondary.main}33`,
        tension: 0.4,
      },
    ],
  };

  const pieData = {
    labels: ['حاضر', 'تأخیر', 'غایب'],
    datasets: [
      {
        data: [data.todayPresent, data.todayDelays, data.todayAbsent],
        backgroundColor: [
          theme.palette.success.main,
          theme.palette.warning.main,
          theme.palette.error.main,
        ],
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' as const } },
  };

  return (
    <Box>
      <LoadingOverlay open={loading} />
      {error && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}
      <Typography variant="h5" sx={{ fontWeight: 700 }} gutterBottom>
        داشبورد مدیریتی
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        شاخص‌های کلیدی عملکرد تیم
      </Typography>

      <Grid container spacing={2.5} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="تعداد کارمندان" value={data.employeeCount} icon={<PeopleIcon />} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="حاضر امروز" value={data.todayPresent} icon={<CheckCircleIcon />} color={theme.palette.success.main} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="تأخیر امروز" value={data.todayDelays} icon={<WarningIcon />} color={theme.palette.warning.main} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="میانگین امتیاز" value={data.averageScore?.toFixed(1) ?? '—'} icon={<StarIcon />} color={theme.palette.info.main} />
        </Grid>
      </Grid>

      <Grid container spacing={2.5} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, md: 6 }}>
          <ChartCard title="شاخص‌های تیم (رادار)">
            {data.teamIndicators.length > 0 ? (
              <Radar data={radarData} options={{ ...chartOptions, scales: { r: { beginAtZero: true, max: 100 } } }} />
            ) : (
              <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
                پس از ثبت ارزیابی یا دریافت MIS، شاخص‌ها نمایش داده می‌شوند.
              </Typography>
            )}
          </ChartCard>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <ChartCard title="وضعیت حضور امروز">
            <Pie data={pieData} options={chartOptions} />
          </ChartCard>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <ChartCard title="روند ماهانه (میله‌ای)">
            <Bar data={barData} options={{ ...chartOptions, plugins: { legend: { display: false } } }} />
          </ChartCard>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <ChartCard title="روند ماهانه (خطی)">
            <Line data={lineData} options={{ ...chartOptions, plugins: { legend: { display: false } } }} />
          </ChartCard>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <ChartCard title="برترین کارمندان" height={280}>
            <List dense>
              {data.topEmployees.map((emp) => (
                <ListItem key={emp.id}>
                  <Avatar sx={{ width: 32, height: 32, ml: 1.5, bgcolor: 'success.main', fontSize: '0.8rem' }}>
                    {emp.ranking}
                  </Avatar>
                  <ListItemText primary={emp.fullName} secondary={`${emp.department ?? '—'} — ${emp.score}`} />
                </ListItem>
              ))}
            </List>
          </ChartCard>
        </Grid>
      </Grid>

      <ChartCard title="کارمندان نیازمند توجه" height="auto">
        <List>
          {data.weakEmployees.map((emp) => (
            <ListItem key={emp.id}>
              <CancelIcon color="error" sx={{ ml: 1 }} />
              <ListItemText primary={emp.fullName} secondary={`${emp.department ?? '—'} — امتیاز: ${emp.score}`} />
            </ListItem>
          ))}
        </List>
      </ChartCard>
    </Box>
  );
}
