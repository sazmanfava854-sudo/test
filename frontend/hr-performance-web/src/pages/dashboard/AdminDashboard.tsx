import { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import LinearProgress from '@mui/material/LinearProgress';
import { useTheme } from '@mui/material/styles';
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
} from 'chart.js';
import { Doughnut } from 'react-chartjs-2';
import PeopleIcon from '@mui/icons-material/People';
import SupervisorAccountIcon from '@mui/icons-material/SupervisorAccount';
import BusinessIcon from '@mui/icons-material/Business';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import StarIcon from '@mui/icons-material/Star';
import StatCard from '../../components/common/StatCard';
import ChartCard from '../../components/common/ChartCard';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import { dashboardService } from '../../services/dashboardService';
import type { AdminDashboardDto } from '../../types';

ChartJS.register(ArcElement, Tooltip, Legend);

const fallbackData: AdminDashboardDto = {
  totalEmployees: 250,
  totalManagers: 18,
  totalDepartments: 12,
  todayPresent: 220,
  todayAbsent: 15,
  averageScore: 81.5,
  departmentRankings: [
    { id: '1', name: 'فناوری اطلاعات', averageScore: 88, employeeCount: 45 },
    { id: '2', name: 'مالی', averageScore: 85, employeeCount: 30 },
    { id: '3', name: 'فروش', averageScore: 82, employeeCount: 55 },
    { id: '4', name: 'منابع انسانی', averageScore: 80, employeeCount: 15 },
    { id: '5', name: 'پشتیبانی', averageScore: 75, employeeCount: 40 },
  ],
  performanceDistribution: [
    { label: 'عالی (۹۰+)', value: 45 },
    { label: 'خوب (۷۵-۹۰)', value: 120 },
    { label: 'متوسط (۶۰-۷۵)', value: 65 },
    { label: 'ضعیف (زیر ۶۰)', value: 20 },
  ],
};

export default function AdminDashboard() {
  const theme = useTheme();
  const [data, setData] = useState<AdminDashboardDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await dashboardService.getAdminDashboard();
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

  const distData = {
    labels: data?.performanceDistribution.map((d) => d.label) ?? [],
    datasets: [
      {
        data: data?.performanceDistribution.map((d) => d.value) ?? [],
        backgroundColor: [
          theme.palette.success.main,
          theme.palette.primary.main,
          theme.palette.warning.main,
          theme.palette.error.main,
        ],
      },
    ],
  };

  return (
    <Box>
      <LoadingOverlay open={loading} />
      <Typography variant="h5" sx={{ fontWeight: 700 }} gutterBottom>
        نمای کلی سازمان
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        آمار و رتبه‌بندی واحدهای سازمانی
      </Typography>

      <Grid container spacing={2.5} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
          <StatCard title="کل کارمندان" value={data?.totalEmployees ?? 0} icon={<PeopleIcon />} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
          <StatCard title="مدیران" value={data?.totalManagers ?? 0} icon={<SupervisorAccountIcon />} color={theme.palette.secondary.main} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
          <StatCard title="واحدها" value={data?.totalDepartments ?? 0} icon={<BusinessIcon />} color={theme.palette.info.main} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
          <StatCard title="حاضر امروز" value={data?.todayPresent ?? 0} icon={<CheckCircleIcon />} color={theme.palette.success.main} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
          <StatCard title="میانگین امتیاز" value={data?.averageScore?.toFixed(1) ?? '—'} icon={<StarIcon />} color={theme.palette.warning.main} />
        </Grid>
      </Grid>

      <Grid container spacing={2.5}>
        <Grid size={{ xs: 12, md: 5 }}>
          <ChartCard title="توزیع عملکرد">
            <Doughnut
              data={distData}
              options={{
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: 'bottom' } },
              }}
            />
          </ChartCard>
        </Grid>
        <Grid size={{ xs: 12, md: 7 }}>
          <ChartCard title="رتبه‌بندی واحدها" height="auto">
            <TableContainer component={Paper} elevation={0}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>واحد</TableCell>
                    <TableCell align="center">تعداد</TableCell>
                    <TableCell>میانگین امتیاز</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data?.departmentRankings.map((dept) => (
                    <TableRow key={dept.id}>
                      <TableCell>{dept.name}</TableCell>
                      <TableCell align="center">{dept.employeeCount}</TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <LinearProgress
                            variant="determinate"
                            value={dept.averageScore}
                            sx={{ flex: 1, height: 8, borderRadius: 4 }}
                          />
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>
                            {dept.averageScore}
                          </Typography>
                        </Box>
                      </TableCell>
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
