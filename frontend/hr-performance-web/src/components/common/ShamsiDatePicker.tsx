import { AdapterDateFnsJalali } from '@mui/x-date-pickers/AdapterDateFnsJalali';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import type { ShamsiDateParts } from '../../utils/misDate';
import { fromShamsiParts, toShamsiParts } from '../../utils/misDate';

interface ShamsiDatePickerProps {
  label: string;
  value: ShamsiDateParts;
  onChange: (value: ShamsiDateParts) => void;
  disabled?: boolean;
}

export default function ShamsiDatePicker({
  label,
  value,
  onChange,
  disabled = false,
}: ShamsiDatePickerProps) {
  return (
    <LocalizationProvider dateAdapter={AdapterDateFnsJalali}>
      <DatePicker
        label={label}
        value={fromShamsiParts(value)}
        disabled={disabled}
        onChange={(date) => {
          if (date && !Number.isNaN(date.getTime())) {
            onChange(toShamsiParts(date));
          }
        }}
        slotProps={{
          textField: {
            fullWidth: true,
          },
        }}
      />
    </LocalizationProvider>
  );
}
