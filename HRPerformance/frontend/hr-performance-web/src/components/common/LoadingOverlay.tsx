import Backdrop from '@mui/material/Backdrop';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import { alpha, useTheme } from '@mui/material/styles';

interface LoadingOverlayProps {
  open: boolean;
  message?: string;
}

export default function LoadingOverlay({
  open,
  message = 'در حال بارگذاری...',
}: LoadingOverlayProps) {
  const theme = useTheme();

  if (!open) return null;

  return (
    <Backdrop
      open={open}
      sx={{
        zIndex: theme.zIndex.modal + 1,
        bgcolor: alpha(theme.palette.background.default, 0.6),
        backdropFilter: 'blur(4px)',
      }}
    >
      <Box sx={{ textAlign: 'center' }}>
        <CircularProgress size={48} thickness={4} />
        <Typography variant="body1" sx={{ mt: 2, fontWeight: 500 }}>
          {message}
        </Typography>
      </Box>
    </Backdrop>
  );
}
