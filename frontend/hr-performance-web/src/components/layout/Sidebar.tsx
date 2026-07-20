import { useLocation, useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Drawer from '@mui/material/Drawer';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import { alpha, useTheme } from '@mui/material/styles';
import DashboardIcon from '@mui/icons-material/Dashboard';
import PeopleIcon from '@mui/icons-material/People';
import AssessmentIcon from '@mui/icons-material/Assessment';
import GavelIcon from '@mui/icons-material/Gavel';
import SettingsIcon from '@mui/icons-material/Settings';
import BarChartIcon from '@mui/icons-material/BarChart';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings';
import { useAppSelector } from '../../store/hooks';
import { selectUserRoles } from '../../store/authSlice';

export const DRAWER_WIDTH = 280;

interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
  roles?: string[];
}

const navItems: NavItem[] = [
  { label: 'داشبورد من', path: '/dashboard', icon: <DashboardIcon /> },
  {
    label: 'داشبورد مدیر',
    path: '/dashboard/manager',
    icon: <TrendingUpIcon />,
    roles: ['Manager', 'OrganizationAdministrator', 'SuperAdministrator'],
  },
  {
    label: 'داشبورد سازمان',
    path: '/dashboard/admin',
    icon: <AdminPanelSettingsIcon />,
    roles: ['OrganizationAdministrator', 'SuperAdministrator'],
  },
  { label: 'کارمندان', path: '/employees', icon: <PeopleIcon />, roles: ['Manager', 'OrganizationAdministrator', 'SuperAdministrator'] },
  {
    label: 'ارزیابی',
    path: '/evaluations',
    icon: <AssessmentIcon />,
    roles: ['Manager', 'OrganizationAdministrator', 'SuperAdministrator'],
  },
  { label: 'اعتراضات', path: '/appeals', icon: <GavelIcon /> },
  { label: 'گزارش‌ها', path: '/reports', icon: <BarChartIcon /> },
  {
    label: 'دریافت MIS',
    path: '/settings?tab=mis',
    icon: <SettingsIcon />,
    roles: ['OrganizationAdministrator', 'SuperAdministrator'],
  },
];

interface SidebarProps {
  onNavigate?: () => void;
}

export default function Sidebar({ onNavigate }: SidebarProps) {
  const theme = useTheme();
  const location = useLocation();
  const navigate = useNavigate();
  const roles = useAppSelector(selectUserRoles);

  const hasRole = (required?: string[]) => {
    if (!required || required.length === 0) return true;
    return required.some((r) => roles.includes(r));
  };

  const visibleItems = navItems.filter((item) => hasRole(item.roles));

  const handleNav = (path: string) => {
    navigate(path);
    onNavigate?.();
  };

  const drawerContent = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ p: 3, pb: 2 }}>
        <Typography
          variant="h6"
          sx={{
            fontWeight: 700,
            background: `linear-gradient(135deg, ${theme.palette.primary.main}, ${theme.palette.secondary.main})`,
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
          }}
        >
          سامانه عملکرد
        </Typography>
        <Typography variant="caption" color="text.secondary">
          مدیریت منابع انسانی
        </Typography>
      </Box>
      <Divider sx={{ opacity: 0.5 }} />
      <List sx={{ flex: 1, px: 1.5, py: 2 }}>
        {visibleItems.map((item) => {
          const isActive =
            location.pathname === item.path ||
            (item.path !== '/dashboard' && location.pathname.startsWith(item.path));
          return (
            <ListItemButton
              key={item.path}
              selected={isActive}
              onClick={() => handleNav(item.path)}
              sx={{
                borderRadius: 2,
                mb: 0.5,
                '&.Mui-selected': {
                  bgcolor: alpha(theme.palette.primary.main, 0.12),
                  '&:hover': {
                    bgcolor: alpha(theme.palette.primary.main, 0.16),
                  },
                },
              }}
            >
              <ListItemIcon
                sx={{
                  minWidth: 40,
                  color: isActive ? 'primary.main' : 'text.secondary',
                }}
              >
                {item.icon}
              </ListItemIcon>
              <ListItemText
                primary={item.label}
                slotProps={{
                  primary: {
                    sx: {
                      fontWeight: isActive ? 600 : 400,
                      fontSize: '0.9rem',
                    },
                  },
                }}
              />
            </ListItemButton>
          );
        })}
      </List>
    </Box>
  );

  return drawerContent;
}

export function SidebarDrawer({
  mobileOpen,
  onClose,
}: {
  mobileOpen: boolean;
  onClose: () => void;
}) {
  const theme = useTheme();

  return (
    <>
      <Drawer
        variant="temporary"
        open={mobileOpen}
        onClose={onClose}
        ModalProps={{ keepMounted: true }}
        sx={{
          display: { xs: 'block', lg: 'none' },
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            boxSizing: 'border-box',
            bgcolor: theme.glass.background,
          },
        }}
      >
        <Sidebar onNavigate={onClose} />
      </Drawer>
      <Drawer
        variant="permanent"
        sx={{
          display: { xs: 'none', lg: 'block' },
          width: DRAWER_WIDTH,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            boxSizing: 'border-box',
            bgcolor: theme.glass.background,
            border: 'none',
            borderLeft: theme.glass.border,
          },
        }}
        open
      >
        <Sidebar />
      </Drawer>
    </>
  );
}
