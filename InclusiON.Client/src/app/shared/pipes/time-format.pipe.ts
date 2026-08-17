import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timeFormat',
  standalone: true,
})
export class TimeFormatPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    if (value === null || value === undefined || isNaN(value)) {
      return '—';
    }

    const totalSeconds = Math.floor(value);
    if (totalSeconds <= 0) {
      return '0 seg';
    }

    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;

    if (minutes > 0 && seconds > 0) {
      return `${minutes} min ${seconds} seg`;
    }

    if (minutes > 0) {
      return `${minutes} min`;
    }

    return `${seconds} seg`;
  }
}
