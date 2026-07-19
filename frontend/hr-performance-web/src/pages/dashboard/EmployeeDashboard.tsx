import { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import Alert from '@mui/material/Alert';
import { useTheme } from '@mui/material/styles';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Filler,
  Tooltip,
  Legend,
} from 'chart.js';
import { Line } from 'react-chartjs-2';
import StarIcon from '@mui/icons-material/Star';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import ThumbUpIcon from '@mui/icons-material/ThumbUp';
import ThumbDownIcon from '@mui/icons-material/ThumbDown';
import EventAvailableIcon from '@mui/icons-material/EventAvailable';
import StatCard from '../../components/common/StatCard';
import ChartCard from '../../components/common/ChartCard';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import { dashboardService } from '../../services/dashboardService';
import type { EmployeeDashboardDto } from '../../types';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Filler,
  Tooltip,
  Legend,
);

const emptyData: EmployeeDashboardDto = {
  currentScore: 0,
  monthlyScore: 0,
  yearlyScore: 0,
  ranking: undefined,
  scoreTrend: [],
  recentAttendance: [],
  positiveCount: 0,
  negativeCount: 0,
};

export default function EmployeeDashboard() {
  const theme = useTheme();
  const [data, setData] = useState<EmployeeDashboardDto>(emptyData);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await dashboardService.getEmployeeDashboard();
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

  const chartData = {
    labels: data.scoreTrend.map((t) => t.label),
    datasets: [
      {
        label: 'امتیاز عملکرد',
        data: data.scoreTrend.map((t) => t.score),
        borderColor: theme.palette.primary.main,
        backgroundColor: `${theme.palette.primary.main}33`,
        fill: true,
        tension: 0.4,
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: {
      y: { min: 0, max: 100, grid: { color: `${theme.palette.divider}` } },
      x: { grid: { display: false } },
    },
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
        داشبورد عملکرد من
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        نمای کلی از عملکرد و حضور شما (از دیتابیس)
      </Typography>

      <Grid container spacing={2.5} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="امتیاز فعلی" value={data.currentScore?.toFixed(1) ?? '—'} icon={<StarIcon />} color={theme.palette.primary.main} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="امتیاز ماهانه" value={data.monthlyScore?.toFixed(1) ?? '—'} icon={<TrendingUpIcon />} color={theme.palette.success.main} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="رتبه سازمانی" value={data.ranking ?? '—'} subtitle="از بین همکاران" icon={<StarIcon />} color={theme.palette.warning.main} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard title="امتیاز سالانه" value={data.yearlyScore?.toFixed(1) ?? '—'} icon={<EventAvailableIcon />} color={theme.palette.info.main} />
        </Grid>
      </Grid>

      <Grid container spacing={2.5}>
        <Grid size={{ xs: 12, md: 8 }}>
          <ChartCard title="روند امتیاز عملکرد" subtitle="از EmployeeScores">
            <Line data={chartData} options={chartOptions} />
          </ChartCard>
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
          <Grid container spacing={2}>
            <Grid size={{ xs: 6, md: 12 }}>
              <StatCard title="امتیازات مثبت" value={data.positiveCount} icon={<ThumbUpIcon />} color={theme.palette.success.main} />
            </Grid>
            <Grid size={{ xs: 6, md: 12 }}>
              <StatCard title="امتیازات منفی" value={data.negativeCount} icon={<ThumbDownIcon />} color={theme.palette.error.main} />
            </Grid>
          </Grid>
        </Grid>
        <Grid size={{ xs: 12 }}>
          <ChartCard title="حضور و غیاب اخیر" height="auto">
            <TableContainer component={Paper} elevation={0}>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>تاریخ</TableCell>
                    <TableCell>وضعیت</TableCell>
                    <TableCell>تأخیر (دقیقه)</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data.recentAttendance.map((row, i) => (
                    <TableRow key={i}>
                      <TableCell>{new Date(row.date).toLocaleDateString('fa-IR')}</TableCell>
                      <TableCell>
                        {row.isAbsent ? (
                          <Chip label="غایب" color="error" size="small" />
                        ) : row.delayMinutes > 0 ? (
                          <Chip label="تأخیر" color="warning" size="small" />
                        ) : (
                          <Chip label="حاضر" color="success" size="small" />
                        )}
                      </TableCell>
                      <TableCell>{row.delayMinutes}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </ChartCard>
        </Grid>
      </Grid>
    </Box>
  );
}
