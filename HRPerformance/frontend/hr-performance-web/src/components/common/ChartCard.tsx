import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import CardHeader from '@mui/material/CardHeader';
import { useTheme } from '@mui/material/styles';
import type { ReactNode } from 'react';
import { glassCardSx } from '../../theme/theme';

interface ChartCardProps {
  title: string;
  subtitle?: string;
  action?: ReactNode;
  children: ReactNode;
  height?: number | string;
}

export default function ChartCard({
  title,
  subtitle,
  action,
  children,
  height = 320,
}: ChartCardProps) {
  const theme = useTheme();

  return (
    <Card sx={{ ...glassCardSx(theme), height: '100%' }}>
      <CardHeader
        title={title}
        subheader={subtitle}
        action={action}
        slotProps={{
          title: { variant: 'h6', sx: { fontWeight: 600 } },
          subheader: { variant: 'body2' },
        }}
        sx={{ pb: 0 }}
      />
      <CardContent sx={{ height, pt: 1 }}>
        {children}
      </CardContent>
    </Card>
  );
}
