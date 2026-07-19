import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Alert from '@mui/material/Alert';
import InputAdornment from '@mui/material/InputAdornment';
import { alpha, useTheme } from '@mui/material/styles';
import PersonIcon from '@mui/icons-material/Person';
import LockIcon from '@mui/icons-material/Lock';
import LoginIcon from '@mui/icons-material/Login';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import {
  login,
  selectAuthError,
  selectAuthLoading,
  clearError,
} from '../../store/authSlice';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import { glassCardSx } from '../../theme/theme';

export default function LoginPage() {
  const theme = useTheme();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const loading = useAppSelector(selectAuthLoading);
  const error = useAppSelector(selectAuthError);
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    dispatch(clearError());
    const result = await dispatch(login({ userName, password }));
    if (login.fulfilled.match(result)) {
      navigate('/dashboard');
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: `linear-gradient(135deg, ${alpha(theme.palette.primary.main, 0.08)} 0%, ${alpha(theme.palette.secondary.main, 0.12)} 50%, ${alpha(theme.palette.primary.dark, 0.06)} 100%)`,
        position: 'relative',
        overflow: 'hidden',
        '&::before': {
          content: '""',
          position: 'absolute',
          width: 400,
          height: 400,
          borderRadius: '50%',
          background: alpha(theme.palette.primary.main, 0.1),
          top: -100,
          right: -100,
          filter: 'blur(60px)',
        },
        '&::after': {
          content: '""',
          position: 'absolute',
          width: 300,
          height: 300,
          borderRadius: '50%',
          background: alpha(theme.palette.secondary.main, 0.1),
          bottom: -80,
          left: -80,
          filter: 'blur(60px)',
        },
      }}
    >
      <LoadingOverlay open={loading} message="در حال ورود..." />

      <Card
        sx={{
          ...glassCardSx(theme),
          width: '100%',
          maxWidth: 440,
          mx: 2,
          position: 'relative',
          zIndex: 1,
        }}
      >
        <CardContent sx={{ p: 4 }}>
          <Box sx={{ textAlign: 'center', mb: 4 }}>
            <Typography
              variant="h4"
              sx={{
                fontWeight: 700,
                background: `linear-gradient(135deg, ${theme.palette.primary.main}, ${theme.palette.secondary.main})`,
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                mb: 1,
              }}
            >
              سامانه عملکرد
            </Typography>
            <Typography variant="body2" color="text.secondary">
              ورود به پنل مدیریت منابع انسانی
            </Typography>
          </Box>

          {error && (
            <Alert severity="error" sx={{ mb: 3 }} onClose={() => dispatch(clearError())}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit}>
            <TextField
              fullWidth
              label="نام کاربری"
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
              required
              sx={{ mb: 2.5 }}
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <PersonIcon color="action" />
                    </InputAdornment>
                  ),
                },
              }}
            />
            <TextField
              fullWidth
              label="رمز عبور"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              sx={{ mb: 3 }}
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <LockIcon color="action" />
                    </InputAdornment>
                  ),
                },
              }}
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              startIcon={<LoginIcon />}
              disabled={loading}
            >
              ورود به سیستم
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}
