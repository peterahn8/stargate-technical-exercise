import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ApiError } from '../../core/api/api-error';
import { AstronautDutyService } from '../../core/api/astronaut-duty.service';
import { AstronautDuty } from '../../core/api/models';
import { dayAfter } from '../../core/duration';
import { dutyRules, notBlank, toIsoDate } from '../../core/validators';

export interface AddDutyData {
  name: string;
  /** The person's current duty, or null if they've never had one. */
  currentDuty: AstronautDuty | null;
}

/** Adds a duty for a person. Closes with true once the API has accepted it (T33). */
@Component({
  selector: 'app-add-duty-dialog',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './add-duty-dialog.html',
  styleUrl: './dialog.scss',
})
export class AddDutyDialog {
  protected readonly data = inject<AddDutyData>(MAT_DIALOG_DATA);
  private readonly dutyService = inject(AstronautDutyService);
  private readonly dialogRef = inject<MatDialogRef<AddDutyDialog, boolean>>(MatDialogRef);

  protected readonly form = new FormGroup(
    {
      rank: new FormControl('', { nonNullable: true, validators: [notBlank] }),
      dutyTitle: new FormControl('', { nonNullable: true, validators: [notBlank] }),
      dutyStartDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
    },
    { validators: [dutyRules(this.data.currentDuty)] },
  );

  /** The earliest date the datepicker offers: the day after the current duty started (D1). */
  protected readonly minDate = this.data.currentDuty
    ? dayAfter(this.data.currentDuty.dutyStartDate)
    : null;

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { rank, dutyTitle, dutyStartDate } = this.form.getRawValue();
    this.saving.set(true);
    this.error.set(null);
    this.dutyService
      .addDuty({
        name: this.data.name,
        rank: rank.trim(),
        dutyTitle: dutyTitle.trim(),
        dutyStartDate: toIsoDate(dutyStartDate!),
      })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (error: ApiError) => {
          this.error.set(error.message);
          this.saving.set(false);
        },
      });
  }
}
