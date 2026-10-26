import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ApiError } from '../../core/api/api-error';
import { PeopleService } from '../../core/api/people.service';
import { notBlank } from '../../core/validators';

/** Adds a person. Closes with their name once the API has accepted it (T33). */
@Component({
  selector: 'app-add-person-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
  ],
  templateUrl: './add-person-dialog.html',
  styleUrl: './dialog.scss',
})
export class AddPersonDialog {
  private readonly peopleService = inject(PeopleService);
  private readonly dialogRef = inject<MatDialogRef<AddPersonDialog, string>>(MatDialogRef);

  protected readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [notBlank] }),
  });
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const name = this.form.controls.name.value.trim();
    this.saving.set(true);
    this.error.set(null);
    this.peopleService.createPerson(name).subscribe({
      next: () => this.dialogRef.close(name),
      error: (error: ApiError) => {
        // E.g. "A person named 'John Doe' already exists." (409).
        this.error.set(error.message);
        this.saving.set(false);
      },
    });
  }
}
