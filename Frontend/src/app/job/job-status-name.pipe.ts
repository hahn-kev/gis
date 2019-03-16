import { Pipe, PipeTransform } from '@angular/core';
import { JobStatus } from './job';

@Pipe({
  name: 'jobStatusName'
})
export class JobStatusNamePipe implements PipeTransform {

  transform(status: JobStatus): string {
    switch (status) {
      case JobStatus.FullTime:
        return 'Full Time';
      case JobStatus.HalfTime:
        return 'Half Time';
      case JobStatus.DailyWorker:
        return 'Daily Worker';
      case JobStatus.SchoolAid:
        return 'School Aide';
      case JobStatus.FullTime10Mo:
        return 'Full Time (10 month)';
      default:
        return status;
    }
  }

}
