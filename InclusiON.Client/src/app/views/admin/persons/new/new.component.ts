import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { filter, take } from 'rxjs';
import {
  PersonsService,
  ProfessionalsService,
  AssignmentsService,
  FamilyService,
  ToastService,
  ValidationService,
} from '@services';
import { AppRoutes, RELATIONSHIPS } from '@shared/constants';
import {
  CreatePersonRequest,
  CreatePersonWithTutorRequest,
  ClassroomResponse,
  FamilyResponse,
  ProfessionalListItemResponse,
} from '@models';
import {
  validDate,
  notFutureDate,
  ageRangeValidator,
  toIsoDate,
  toInputDate,
  uniqueDocumentValidator,
  uniqueEmailValidator,
  notEqualToControlValidator,
} from '@shared/utils';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  FormSelectDirective,
  RowComponent,
  ModalComponent,
  ModalHeaderComponent,
  ModalBodyComponent,
  ModalFooterComponent,
  AlertComponent,
} from '@coreui/angular';
import { OnlyNumbersDirective } from '@shared/directives';
import { SearchableSelectComponent } from '@shared/components/searchable-select/searchable-select.component';

@Component({
  selector: 'app-new',
  imports: [
    ReactiveFormsModule,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    FormFeedbackComponent,
    FormSelectDirective,
    ButtonDirective,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    AlertComponent,
    OnlyNumbersDirective,
    SearchableSelectComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly familyService = inject(FamilyService);
  private readonly validationService = inject(ValidationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  submitted = false;
  submittedStep1 = false;
  submittedStep2 = false;
  serverError = '';
  currentStep = 1;
  tutorMode: 'new' | 'existing' = 'new';

  showPasswordModal = false;
  tempPassword = '';
  tempPasswordEmail = '';
  createdPersonId: string | null = null;

  professionals: ProfessionalListItemResponse[] = [];
  classrooms: ClassroomResponse[] = [];

  readonly relationships = RELATIONSHIPS;

  // Límites dinámicos de fecha de nacimiento (entre 12 y 40 años)
  readonly minBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 40, today.getMonth(), today.getDate()));
  })();

  readonly maxBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 12, today.getMonth(), today.getDate()));
  })();

  private readonly newTutorEmailAsyncValidator = uniqueEmailValidator(
    email => this.validationService.checkEmail(email),
  );

  private readonly newTutorDocAsyncValidator = uniqueDocumentValidator(
    doc => this.validationService.checkDocument(doc),
  );

  form: FormGroup = this.fb.group({
    // Alumno - Datos personales
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    documentNumber: [
      '',
      [Validators.required, Validators.minLength(7), Validators.maxLength(8), Validators.pattern(/^[0-9]+$/)],
      [uniqueDocumentValidator(doc => this.validationService.checkDocument(doc))],
    ],
    birthDate: ['', [Validators.required, validDate, notFutureDate, ageRangeValidator(12, 40)]],

    // Tutor - Modo existente
    existingTutorId: [null],

    // Tutor - Datos personales (modo nuevo)
    tutorFirstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    tutorLastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    tutorEmail: ['', [Validators.required, Validators.email], [this.newTutorEmailAsyncValidator]],
    tutorDocumentNumber: [
      '',
      [
        Validators.required,
        Validators.minLength(7),
        Validators.maxLength(8),
        Validators.pattern(/^[0-9]+$/),
        notEqualToControlValidator('documentNumber', 'sameDocument'),
      ],
      [this.newTutorDocAsyncValidator],
    ],
    tutorPhone: ['', [Validators.maxLength(20), Validators.pattern(/^[0-9]+$/)]],
    tutorRelationship: ['', [Validators.required, Validators.maxLength(50)]],

    // Asignación obligatoria de Profesional y opcional de Aula
    selectedProfessionalId: [null, [Validators.required]],
    selectedClassroomId: [{ value: null, disabled: true }],
  });

  readonly searchFamiliesFn = (query: string) =>
    this.familyService.getAvailableFamilies(query, 1, 20);
  readonly displayFamily = (f: FamilyResponse) => f.fullName ?? '';
  readonly subDisplayFamily = (f: FamilyResponse) =>
    f.documentNumber ? `${f.email ?? ''} (DNI: ${f.documentNumber})` : (f.email ?? '');
  readonly valueFromFamily = (f: FamilyResponse) => f.id;

  get f() {
    return this.form.controls;
  }

  get tutorF() {
    return {
      existingTutorId: this.form.get('existingTutorId')!,
      tutorFirstName: this.form.get('tutorFirstName')!,
      tutorLastName: this.form.get('tutorLastName')!,
      tutorEmail: this.form.get('tutorEmail')!,
      tutorDocumentNumber: this.form.get('tutorDocumentNumber')!,
      tutorPhone: this.form.get('tutorPhone')!,
      tutorRelationship: this.form.get('tutorRelationship')!,
    };
  }

  get classroomF() {
    return {
      selectedProfessionalId: this.form.get('selectedProfessionalId')!,
      selectedClassroomId: this.form.get('selectedClassroomId')!,
    };
  }

  ngOnInit(): void {
    this.professionalsService.getProfessionals({ page: 1, pageSize: 200, status: 'active' }).subscribe({
      next: (res) => this.professionals = res.data,
    });

    // Revalidar comparación de DNI del tutor si cambia el DNI del alumno
    this.form.get('documentNumber')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (this.tutorMode === 'new' && this.form.get('tutorDocumentNumber')?.value) {
          this.form.get('tutorDocumentNumber')?.updateValueAndValidity({ emitEvent: false });
        }
      });
  }

  showStep1Error(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    if (!control || !control.invalid) return false;
    if (control.errors?.['documentExists']) return true;
    return this.submittedStep1 || control.touched;
  }

  showStep2Error(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    if (!control || !control.invalid) return false;
    if (control.errors?.['emailExists'] || control.errors?.['documentExists'] || control.errors?.['sameDocument']) {
      return true;
    }
    return this.submittedStep2 || control.touched;
  }

  setTutorMode(mode: 'new' | 'existing'): void {
    if (this.tutorMode === mode) return;
    this.tutorMode = mode;
    this.submittedStep2 = false;
    this.serverError = '';

    const existingCtrl = this.form.get('existingTutorId')!;
    const firstNameCtrl = this.form.get('tutorFirstName')!;
    const lastNameCtrl = this.form.get('tutorLastName')!;
    const emailCtrl = this.form.get('tutorEmail')!;
    const docCtrl = this.form.get('tutorDocumentNumber')!;
    const phoneCtrl = this.form.get('tutorPhone')!;

    if (mode === 'existing') {
      existingCtrl.setValidators([Validators.required]);

      firstNameCtrl.clearValidators();
      lastNameCtrl.clearValidators();
      emailCtrl.clearValidators();
      emailCtrl.clearAsyncValidators();
      docCtrl.clearValidators();
      docCtrl.clearAsyncValidators();
      phoneCtrl.clearValidators();

      firstNameCtrl.setValue('');
      lastNameCtrl.setValue('');
      emailCtrl.setValue('');
      docCtrl.setValue('');
      phoneCtrl.setValue('');
    } else {
      existingCtrl.clearValidators();
      existingCtrl.setValue(null);

      firstNameCtrl.setValidators([Validators.required, Validators.minLength(2), Validators.maxLength(100)]);
      lastNameCtrl.setValidators([Validators.required, Validators.minLength(2), Validators.maxLength(100)]);
      emailCtrl.setValidators([Validators.required, Validators.email]);
      emailCtrl.setAsyncValidators([this.newTutorEmailAsyncValidator]);
      docCtrl.setValidators([
        Validators.required,
        Validators.minLength(7),
        Validators.maxLength(8),
        Validators.pattern(/^[0-9]+$/),
        notEqualToControlValidator('documentNumber', 'sameDocument'),
      ]);
      docCtrl.setAsyncValidators([this.newTutorDocAsyncValidator]);
      phoneCtrl.setValidators([Validators.maxLength(20), Validators.pattern(/^[0-9]+$/)]);
    }

    [existingCtrl, firstNameCtrl, lastNameCtrl, emailCtrl, docCtrl, phoneCtrl].forEach(c => {
      c.markAsUntouched();
      c.updateValueAndValidity();
    });
  }

  nextToStep2(): void {
    this.submittedStep1 = true;
    const studentControls = ['firstName', 'lastName', 'documentNumber', 'birthDate'];
    studentControls.forEach(name => this.form.get(name)?.markAsTouched());

    const docCtrl = this.form.get('documentNumber')!;
    if (docCtrl.pending) {
      docCtrl.statusChanges.pipe(filter(s => s !== 'PENDING'), take(1)).subscribe(() => {
        this.validateAndAdvanceStep1();
      });
      return;
    }

    this.validateAndAdvanceStep1();
  }

  private validateAndAdvanceStep1(): void {
    const studentControls = ['firstName', 'lastName', 'documentNumber', 'birthDate'];
    const isStudentValid = studentControls.every(name => this.form.get(name)?.valid);
    if (isStudentValid) {
      this.currentStep = 2;
    }
  }

  nextToStep3(): void {
    this.submittedStep2 = true;

    const tutorControls = this.tutorMode === 'existing'
      ? ['existingTutorId', 'tutorRelationship']
      : ['tutorFirstName', 'tutorLastName', 'tutorEmail', 'tutorDocumentNumber', 'tutorPhone', 'tutorRelationship'];

    tutorControls.forEach(name => this.form.get(name)?.markAsTouched());

    const anyPending = tutorControls.some(name => this.form.get(name)?.pending);
    if (anyPending) {
      this.form.statusChanges.pipe(filter(s => s !== 'PENDING'), take(1)).subscribe(() => {
        this.validateAndAdvanceStep2(tutorControls);
      });
      return;
    }

    this.validateAndAdvanceStep2(tutorControls);
  }

  private validateAndAdvanceStep2(tutorControls: string[]): void {
    const isTutorValid = tutorControls.every(name => this.form.get(name)?.valid);
    if (isTutorValid) {
      this.currentStep = 3;
    }
  }

  prevStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  onProfessionalChange(): void {
    const profId = this.form.get('selectedProfessionalId')?.value;
    const classroomCtrl = this.form.get('selectedClassroomId');
    classroomCtrl?.setValue(null);
    this.classrooms = [];
    if (profId) {
      classroomCtrl?.enable();
      this.assignmentsService.getClassroomsByProfessional(profId).subscribe({
        next: (data) => this.classrooms = data,
      });
    } else {
      classroomCtrl?.disable();
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';
    this.form.markAllAsTouched();

    if (this.form.pending) {
      this.form.statusChanges.pipe(filter(s => s !== 'PENDING'), take(1)).subscribe(() => {
        this.attemptSubmit();
      });
      return;
    }

    this.attemptSubmit();
  }

  private attemptSubmit(): void {
    if (this.form.invalid) return;

    const raw = this.form.getRawValue();
    const studentRequest: CreatePersonRequest = {
      firstName: raw.firstName.trim(),
      lastName: raw.lastName.trim(),
      birthDate: toIsoDate(raw.birthDate),
      ...(raw.documentNumber && { documentNumber: raw.documentNumber.trim() }),
    };

    const request: CreatePersonWithTutorRequest = this.tutorMode === 'existing'
      ? {
        student: studentRequest,
        existingTutorId: raw.existingTutorId,
        tutorRelationship: raw.tutorRelationship,
        professionalId: raw.selectedProfessionalId,
        ...(raw.selectedClassroomId ? { classroomId: raw.selectedClassroomId } : {}),
      }
      : {
        student: studentRequest,
        tutorFirstName: raw.tutorFirstName.trim(),
        tutorLastName: raw.tutorLastName.trim(),
        tutorEmail: raw.tutorEmail.trim(),
        ...(raw.tutorDocumentNumber && { tutorDocumentNumber: raw.tutorDocumentNumber.trim() }),
        ...(raw.tutorPhone && { tutorPhone: raw.tutorPhone.trim() }),
        tutorRelationship: raw.tutorRelationship,
        professionalId: raw.selectedProfessionalId,
        ...(raw.selectedClassroomId ? { classroomId: raw.selectedClassroomId } : {}),
      };

    this.personsService.createPersonWithTutor(request).subscribe({
      next: (person) => {
        if (person.tutorTemporaryPassword) {
          this.tempPassword = person.tutorTemporaryPassword;
          this.tempPasswordEmail = raw.tutorEmail;
          this.createdPersonId = person.id;
          this.showPasswordModal = true;
        } else {
          this.router.navigate([AppRoutes.Admin.Persons, person.id]);
        }
      },
      error: (err) => {
        this.serverError = err?.userMessage || 'Error al crear el alumno y tutor';
      },
    });
  }

  onPasswordModalVisibleChange(visible: boolean): void {
    this.showPasswordModal = visible;
    if (!visible && this.createdPersonId) {
      this.router.navigate([AppRoutes.Admin.Persons, this.createdPersonId]);
    }
  }

  closePasswordModal(): void {
    this.onPasswordModalVisibleChange(false);
  }

  copyPassword(): void {
    navigator.clipboard.writeText(this.tempPassword).then(() => {
      this.toastService.success('Contraseña copiada al portapapeles');
    });
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Persons]);
  }
}
