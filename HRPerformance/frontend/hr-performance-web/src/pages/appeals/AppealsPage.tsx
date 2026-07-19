import { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import AddIcon from '@mui/icons-material/Add';
import api from '../../services/api';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import { useAppSelector } from '../../store/hooks';
import { selectUserRoles } from '../../store/authSlice';
import {
  type AppealDto,
  type CreateAppealRequest,
  AppealStatus,
  APPEAL_STATUS_LABELS,
} from '../../types';

const statusColor: Record<AppealStatus, 'warning' | 'success' | 'error'> = {
  [AppealStatus.Pending]: 'warning',
  [AppealStatus.Approved]: 'success',
  [AppealStatus.Rejected]: 'error',
};

export default function AppealsPage() {
  const roles = useAppSelector(selectUserRoles);
  const isManager = roles.some((r) =>
    ['Manager', 'OrganizationAdministrator', 'SuperAdministrator'].includes(r),
  );

  const [appeals, setAppeals] = useState<AppealDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [reviewDialog, setReviewDialog] = useState<AppealDto | null>(null);
  const [reason, setReason] = useState('');
  const [reviewStatus, setReviewStatus] = useState<AppealStatus>(AppealStatus.Approved);
  const [reviewComments, setReviewComments] = useState('');

  const fetchAppeals = async () => {
    setLoading(true);
    try {
      const { data } = await api.get('/appeals');
      const items = data?.data ?? data;
      if (Array.isArray(items)) setAppeals(items);
    } catch {
      setAppeals([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAppeals();
  }, []);

  const handleCreate = async () => {
    const request: CreateAppealRequest = { reason };
    await api.post('/appeals', request);
    setDialogOpen(false);
    setReason('');
    fetchAppeals();
  };

  const handleReview = async () => {
    if (!reviewDialog) return;
    await api.put('/appeals/review', {
      appealId: reviewDialog.id,
      status: reviewStatus,
      reviewComments,
    });
    setReviewDialog(null);
    setReviewComments('');
    fetchAppeals();
  };

  return (
    <Box>
      <LoadingOverlay open={loading} />
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700 }}>
            اعتراضات
          </Typography>
          <Typography variant="body2" color="text.secondary">
            مدیریت درخواست‌های اعتراض به امتیاز
          </Typography>
        </Box>
        {!isManager && (
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setDialogOpen(true)}
          >
            ثبت اعتراض جدید
          </Button>
        )}
      </Box>

      <TableContainer component={Paper} elevation={0}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>کارمند</TableCell>
              <TableCell>دلیل</TableCell>
              <TableCell>وضعیت</TableCell>
              <TableCell>تاریخ</TableCell>
              {isManager && <TableCell>عملیات</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {appeals.map((appeal) => (
              <TableRow key={appeal.id}>
                <TableCell>{appeal.employeeName}</TableCell>
                <TableCell>{appeal.reason}</TableCell>
                <TableCell>
                  <Chip
                    label={APPEAL_STATUS_LABELS[appeal.status]}
                    color={statusColor[appeal.status]}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  {new Date(appeal.createdAt).toLocaleDateString('fa-IR')}
                </TableCell>
                {isManager && (
                  <TableCell>
                    {appeal.status === AppealStatus.Pending && (
                      <Button
                        size="small"
                        onClick={() => setReviewDialog(appeal)}
                      >
                        بررسی
                      </Button>
                    )}
                  </TableCell>
                )}
              </TableRow>
            ))}
            {appeals.length === 0 && !loading && (
              <TableRow>
                <TableCell colSpan={isManager ? 5 : 4} align="center">
                  اعتراضی ثبت نشده است
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>ثبت اعتراض جدید</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            multiline
            rows={4}
            label="دلیل اعتراض"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>انصراف</Button>
          <Button variant="contained" onClick={handleCreate} disabled={!reason.trim()}>
            ثبت
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={!!reviewDialog} onClose={() => setReviewDialog(null)} maxWidth="sm" fullWidth>
        <DialogTitle>بررسی اعتراض</DialogTitle>
        <DialogContent>
          <TextField
            select
            fullWidth
            label="نتیجه"
            value={reviewStatus}
            onChange={(e) => setReviewStatus(Number(e.target.value) as AppealStatus)}
            sx={{ mt: 1, mb: 2 }}
          >
            <MenuItem value={AppealStatus.Approved}>تأیید</MenuItem>
            <MenuItem value={AppealStatus.Rejected}>رد</MenuItem>
          </TextField>
          <TextField
            fullWidth
            multiline
            rows={3}
            label="توضیحات بررسی"
            value={reviewComments}
            onChange={(e) => setReviewComments(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setReviewDialog(null)}>انصراف</Button>
          <Button variant="contained" onClick={handleReview}>
            ثبت نتیجه
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
