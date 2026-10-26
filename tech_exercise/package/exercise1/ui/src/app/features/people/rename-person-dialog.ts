import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ApiError } from '../../core/api/api-error';
import { PeopleService } from '../../core/api/people.service';
import { notBlank } from '../../core/validators';

export interface RenamePersonData {
  name: string;
}

/** Renames a person (D16). Closes with the new name once the API has accepted it (T40). */
@Component({
  selector: 'app-rename-person-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
  ],
  templateUrl: './rename-person-dialog.html',
  styleUrl: './dialog.scss',
})
export class RenamePersonDialog {
  protected readonly data = inject<RenamePersonData>(MAT_DIALOG_DATA);
  private readonly peopleService = inject(PeopleService);
  private readonly dialogRef = inject<MatDialogRef<RenamePersonDialog, string>>(MatDialogRef);

  protected readonly form = new FormGroup({
    newName: new FormControl(this.data.name, { nonNullable: true, validators: [notBlank] }),
  });
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const newName = this.form.controls.newName.value.trim();
    this.saving.set(true);
    this.error.set(null);
    this.peopleService.renamePerson(this.data.name, newName).subscribe({
      next: () => this.dialogRef.close(newName),
      error: (error: ApiError) => {
        // E.g. "A person named 'Jane Doe' already exists." (409).
        this.error.set(error.message);
        this.saving.set(false);
      },
    });
  }
}
