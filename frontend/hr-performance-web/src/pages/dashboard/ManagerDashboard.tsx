import { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Avatar from '@mui/material/Avatar';
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
);

const fallbackData: ManagerDashboardDto = {
  employeeCount: 45,
  todayPresent: 38,
  todayDelays: 5,
  todayAbsent: 2,
  averageScore: 82.4,
  topEmployees: [
    { id: '1', fullName: 'علی محمدی', department: 'فناوری', score: 95, ranking: 1 },
    { id: '2', fullName: 'سارا احمدی', department: 'مالی', score: 92, ranking: 2 },
    { id: '3', fullName: 'رضا کریمی', department: 'فروش', score: 90, ranking: 3 },
  ],
  weakEmployees: [
    { id: '4', fullName: 'مریم حسینی', department: 'پشتیبانی', score: 55, ranking: 43 },
    { id: '5', fullName: 'حسین رضایی', department: 'انبار', score: 58, ranking: 42 },
  ],
  monthlyTrend: [
    { label: 'فروردین', value: 78 },
    { label: 'اردیبهشت', value: 80 },
    { label: 'خرداد', value: 82 },
    { label: 'تیر', value: 81 },
    { label: 'مرداد', value: 84 },
    { label: 'شهریور', value: 85 },
  ],
};

export default function ManagerDashboard() {
  const theme = useTheme();
  const [data, setData] = useState<ManagerDashboardDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await dashboardService.getManagerDashboard();
        if (response.success && response.data) {
          setData(response.data);
        } else {
          setData(fallbackData);
        }
      } catch {
        setData(fallbackData);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const labels = data?.monthlyTrend.map((t) => t.label) ?? [];
  const values = data?.monthlyTrend.map((t) => t.value) ?? [];

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
        data: [
          data?.todayPresent ?? 0,
          data?.todayDelays ?? 0,
          data?.todayAbsent ?? 0,
        ],
        backgroundColor: [
          theme.palette.success.main,
          theme.palette.warning.main,
          theme.palette.error.main,
        ],
      },
    ],
  };

  const radarData = {
    labels: ['حضور', 'کیفیت', 'سرعت', 'همکاری', 'نوآوری'],
    datasets: [
      {
        label: 'عملکرد تیم',
        data: [85, 78, 82, 90, 75],
        backgroundColor: `${theme.palette.primary.main}40`,
        borderColor: theme.palette.primary.main,
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
      <Typography variant="h5" sx={{ fontWeight: 700 }} gutterBottom>
        داشبورد مدیریتی
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        شاخص‌های کلیدی عملکرد تیم
      </Typography>

      <Grid container spacing={2.5} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="تعداد کارمندان"
            value={data?.employeeCount ?? 0}
            icon={<PeopleIcon />}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="حاضر امروز"
            value={data?.todayPresent ?? 0}
            icon={<CheckCircleIcon />}
            color={theme.palette.success.main}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="تأخیر امروز"
            value={data?.todayDelays ?? 0}
            icon={<WarningIcon />}
            color={theme.palette.warning.main}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="میانگین امتیاز"
            value={data?.averageScore?.toFixed(1) ?? '—'}
            icon={<StarIcon />}
            color={theme.palette.info.main}
          />
        </Grid>
      </Grid>

      <Grid container spacing={2.5} sx={{ mb: 3 }}>
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
        <Grid size={{ xs: 12, md: 4 }}>
          <ChartCard title="وضعیت حضور امروز">
            <Pie data={pieData} options={chartOptions} />
          </ChartCard>
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
          <ChartCard title="شاخص‌های تیم (رادار)">
            <Radar data={radarData} options={chartOptions} />
          </ChartCard>
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
          <ChartCard title="برترین کارمندان" height={280}>
            <List dense>
              {data?.topEmployees.map((emp) => (
                <ListItem key={emp.id}>
                  <Avatar sx={{ width: 32, height: 32, ml: 1.5, bgcolor: 'success.main', fontSize: '0.8rem' }}>
                    {emp.ranking}
                  </Avatar>
                  <ListItemText
                    primary={emp.fullName}
                    secondary={`${emp.department} — ${emp.score}`}
                  />
                </ListItem>
              ))}
            </List>
          </ChartCard>
        </Grid>
      </Grid>

      <ChartCard title="کارمندان نیازمند توجه" height="auto">
        <List>
          {data?.weakEmployees.map((emp) => (
            <ListItem key={emp.id}>
              <CancelIcon color="error" sx={{ ml: 1 }} />
              <ListItemText
                primary={emp.fullName}
                secondary={`${emp.department} — امتیاز: ${emp.score}`}
              />
            </ListItem>
          ))}
        </List>
      </ChartCard>
    </Box>
  );
}
