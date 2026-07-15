import { useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import Avatar from '@mui/material/Avatar';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Tooltip from '@mui/material/Tooltip';
import { alpha, useTheme } from '@mui/material/styles';
import MenuIcon from '@mui/icons-material/Menu';
import Brightness4Icon from '@mui/icons-material/Brightness4';
import Brightness7Icon from '@mui/icons-material/Brightness7';
import LogoutIcon from '@mui/icons-material/Logout';
import PersonIcon from '@mui/icons-material/Person';
import { SidebarDrawer, DRAWER_WIDTH } from './Sidebar';
import { useAppTheme } from '../../theme/ThemeProvider';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { logout, selectUser } from '../../store/authSlice';

export default function MainLayout() {
  const theme = useTheme();
  const { mode, toggleTheme } = useAppTheme();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const user = useAppSelector(selectUser);
  const [mobileOpen, setMobileOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <SidebarDrawer
        mobileOpen={mobileOpen}
        onClose={() => setMobileOpen(false)}
      />

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          width: { lg: `calc(100% - ${DRAWER_WIDTH}px)` },
          minHeight: '100vh',
          bgcolor: 'background.default',
        }}
      >
        <AppBar
          position="sticky"
          elevation={0}
          sx={{
            bgcolor: theme.glass.background,
            borderBottom: theme.glass.border,
            color: 'text.primary',
          }}
        >
          <Toolbar>
            <IconButton
              color="inherit"
              edge="start"
              onClick={() => setMobileOpen(true)}
              sx={{ display: { lg: 'none' }, mr: 1 }}
            >
              <MenuIcon />
            </IconButton>

            <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 600 }}>
              سامانه مدیریت عملکرد کارکنان
            </Typography>

            <Tooltip title={mode === 'light' ? 'حالت تاریک' : 'حالت روشن'}>
              <IconButton color="inherit" onClick={toggleTheme} sx={{ mr: 1 }}>
                {mode === 'light' ? <Brightness4Icon /> : <Brightness7Icon />}
              </IconButton>
            </Tooltip>

            <Tooltip title="حساب کاربری">
              <IconButton
                onClick={(e) => setAnchorEl(e.currentTarget)}
                sx={{ p: 0.5 }}
              >
                <Avatar
                  sx={{
                    width: 36,
                    height: 36,
                    bgcolor: alpha(theme.palette.primary.main, 0.15),
                    color: 'primary.main',
                    fontSize: '0.9rem',
                  }}
                >
                  {user?.firstName?.charAt(0) ?? 'ک'}
                </Avatar>
              </IconButton>
            </Tooltip>

            <Menu
              anchorEl={anchorEl}
              open={Boolean(anchorEl)}
              onClose={() => setAnchorEl(null)}
              transformOrigin={{ horizontal: 'left', vertical: 'top' }}
              anchorOrigin={{ horizontal: 'left', vertical: 'bottom' }}
            >
              <Box sx={{ px: 2, py: 1 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                  {user?.fullName}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {user?.email}
                </Typography>
              </Box>
              <MenuItem onClick={() => setAnchorEl(null)}>
                <PersonIcon fontSize="small" sx={{ ml: 1 }} />
                پروفایل
              </MenuItem>
              <MenuItem
                onClick={() => {
                  setAnchorEl(null);
                  handleLogout();
                }}
              >
                <LogoutIcon fontSize="small" sx={{ ml: 1 }} />
                خروج
              </MenuItem>
            </Menu>
          </Toolbar>
        </AppBar>

        <Box sx={{ p: { xs: 2, md: 3 } }}>
          <Outlet />
        </Box>
      </Box>
    </Box>
  );
}
