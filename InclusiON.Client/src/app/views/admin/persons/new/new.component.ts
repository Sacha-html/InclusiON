import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CatalogsService, PersonsService, ProfessionalsService, AssignmentsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { CatalogItem, AutonomyLevelItem, LoginMethodItem, CreatePersonRequest, CreatePersonWithTutorRequest, ClassroomResponse, ProfessionalListItemResponse } from '@models';
import { validDate, notFutureDate, ageRangeValidator, toIsoDate, toInputDate } from '@shared/utils';
import { AvatarColorPickerComponent } from '@shared/components';
import { NgClass } from '@angular/common';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormSelectDirective,
  RowComponent,
  ModalComponent,
  ModalHeaderComponent,
  ModalBodyComponent,
  ModalFooterComponent,
  AlertComponent,
} from '@coreui/angular';

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
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    FormSelectDirective,
    ButtonDirective,
    AvatarColorPickerComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    AlertComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly toastService = inject(ToastService);

  submitted = false;
  submittedStep1 = false;
  submittedStep2 = false;
  serverError = '';
  currentStep = 1;

  showPasswordModal = false;
  tempPassword = '';
  tempPasswordEmail = '';
  createdPersonId: string | null = null;

  disabilityTypes: CatalogItem[] = [];
  autonomyLevels: AutonomyLevelItem[] = [];
  loginMethods: LoginMethodItem[] = [];
  professionals: ProfessionalListItemResponse[] = [];
  classrooms: ClassroomResponse[] = [];

  // Límites dinámicos de fecha de nacimiento (entre 12 y 40 años)
  readonly minBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 40, today.getMonth(), today.getDate()));
  })();

  readonly maxBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 12, today.getMonth(), today.getDate()));
  })();

  form: FormGroup = this.fb.group({
    // Alumno - Datos personales
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    documentNumber: ['', [Validators.minLength(6), Validators.maxLength(20), Validators.pattern(/^[a-zA-Z0-9]+$/)]],
    birthDate: ['', [Validators.required, validDate, notFutureDate, ageRangeValidator(12, 40)]],
    // Alumno - Discapacidad
    disabilityTypeId: [null, [Validators.required]],
    // Alumno - Perfil funcional
    attentionLevel: [null],
    communicationLevel: [null],
    motorSkillLevel: [null],
    usesAAC: [false],
    usesSignLanguage: [false],
    // Alumno - Preferencias
    interestsAndMotivators: [''],
    learningStyle: [''],
    availableResources: [''],
    additionalTherapies: [''],
    // Alumno - Accesibilidad
    requiresLargeFont: [false],
    requiresHighContrast: [false],
    visualNoiseSensitivity: [false],
    soundSensitivity: [false],
    colorBlindnessType: [null],
    // Alumno - Configuración de acceso
    autonomyLevelId: [null],
    loginMethodId: [null],
    pin: ['', [Validators.pattern(/^\d{4}$/)]],
    avatarColor: ['#2196F3'],

    // Tutor - Datos personales
    tutorFirstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    tutorLastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    tutorEmail: ['', [Validators.required, Validators.email]],
    tutorDocumentNumber: ['', [Validators.minLength(6), Validators.maxLength(20), Validators.pattern(/^[a-zA-Z0-9]+$/)]],
    tutorPhone: ['', [Validators.maxLength(20)]],
    tutorRelationship: ['', [Validators.required, Validators.maxLength(50)]],

    // Asignación de Profesional y Aula (Obligatorio)
    selectedProfessionalId: [null, [Validators.required]],
    selectedClassroomId: [{ value: null, disabled: true }, [Validators.required]],
  });

  get f() {
    return this.form.controls;
  }

  get tutorF() {
    return {
      tutorFirstName: this.form.get('tutorFirstName')!,
      tutorLastName: this.form.get('tutorLastName')!,
      tutorEmail: this.form.get('tutorEmail')!,
      tutorDocumentNumber: this.form.get('tutorDocumentNumber')!,
      tutorPhone: this.form.get('tutorPhone')!,
      tutorRelationship: this.form.get('tutorRelationship')!
    };
  }

  get classroomF() {
    return {
      selectedProfessionalId: this.form.get('selectedProfessionalId')!,
      selectedClassroomId: this.form.get('selectedClassroomId')!
    };
  }

  get selectedLoginMethod(): LoginMethodItem | undefined {
    const id = this.form.get('loginMethodId')?.value;
    return id ? this.loginMethods.find(m => m.id === +id) : undefined;
  }

  ngOnInit(): void {
    this.catalogsService.getDisabilityTypes().subscribe({
      next: (data) => this.disabilityTypes = data,
    });
    this.catalogsService.getAutonomyLevels().subscribe({
      next: (data) => this.autonomyLevels = data,
    });
    this.catalogsService.getLoginMethods().subscribe({
      next: (data) => this.loginMethods = data.filter(m => m.code !== 'STANDARD' && m.id !== 1),
    });
    this.professionalsService.getProfessionals({ page: 1, pageSize: 200, status: 'active' }).subscribe({
      next: (res) => this.professionals = res.data,
    });

    this.form.get('loginMethodId')?.valueChanges.subscribe((val) => {
      const pinCtrl = this.form.get('pin');
      const methodId = val ? +val : null;
      if (methodId === 2) {
        pinCtrl?.setValidators([Validators.required, Validators.pattern(/^\d{4}$/)]);
        pinCtrl?.enable();
      } else {
        pinCtrl?.clearValidators();
        pinCtrl?.setValue('');
        pinCtrl?.disable();
      }
      pinCtrl?.updateValueAndValidity();
    });
  }

  nextToStep2(): void {
    this.submittedStep1 = true;
    
    const studentControls = [
      'firstName', 'lastName', 'documentNumber', 'birthDate', 'disabilityTypeId', 'pin'
    ];
    let isStudentValid = true;
    for (const ctrlName of studentControls) {
      const ctrl = this.form.get(ctrlName);
      if (ctrl && ctrl.invalid) {
        ctrl.markAsTouched();
        isStudentValid = false;
      }
    }

    if (isStudentValid) {
      this.currentStep = 2;
    }
  }

  nextToStep3(): void {
    this.submittedStep2 = true;

    const tutorControls = [
      'tutorFirstName', 'tutorLastName', 'tutorEmail', 'tutorDocumentNumber', 'tutorPhone', 'tutorRelationship'
    ];
    let isTutorValid = true;
    for (const ctrlName of tutorControls) {
      const ctrl = this.form.get(ctrlName);
      if (ctrl && ctrl.invalid) {
        ctrl.markAsTouched();
        isTutorValid = false;
      }
    }

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

    if (this.form.invalid) return;

    const raw = this.form.getRawValue();
    const studentRequest: CreatePersonRequest = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      birthDate: toIsoDate(raw.birthDate),
      usesAAC: raw.usesAAC ?? false,
      usesSignLanguage: raw.usesSignLanguage ?? false,
      requiresLargeFont: raw.requiresLargeFont ?? false,
      requiresHighContrast: raw.requiresHighContrast ?? false,
      visualNoiseSensitivity: raw.visualNoiseSensitivity ?? false,
      soundSensitivity: raw.soundSensitivity ?? false,
      ...(raw.colorBlindnessType && { colorBlindnessType: raw.colorBlindnessType }),
      ...(raw.documentNumber && { documentNumber: raw.documentNumber }),
      disabilityTypeId: +raw.disabilityTypeId,
      ...(raw.attentionLevel && { attentionLevel: +raw.attentionLevel }),
      ...(raw.communicationLevel && { communicationLevel: +raw.communicationLevel }),
      ...(raw.motorSkillLevel && { motorSkillLevel: +raw.motorSkillLevel }),
      ...(raw.interestsAndMotivators && { interestsAndMotivators: raw.interestsAndMotivators }),
      ...(raw.learningStyle && { learningStyle: raw.learningStyle }),
      ...(raw.availableResources && { availableResources: raw.availableResources }),
      ...(raw.additionalTherapies && { additionalTherapies: raw.additionalTherapies }),
      ...(raw.autonomyLevelId && { autonomyLevelId: +raw.autonomyLevelId }),
      ...(raw.loginMethodId && { loginMethodId: +raw.loginMethodId }),
      ...(raw.pin && { pin: raw.pin }),
      ...(raw.avatarColor && { avatarColor: raw.avatarColor }),
    };

    const request: CreatePersonWithTutorRequest = {
      student: studentRequest,
      tutorFirstName: raw.tutorFirstName,
      tutorLastName: raw.tutorLastName,
      tutorEmail: raw.tutorEmail,
      ...(raw.tutorDocumentNumber && { tutorDocumentNumber: raw.tutorDocumentNumber }),
      ...(raw.tutorPhone && { tutorPhone: raw.tutorPhone }),
      tutorRelationship: raw.tutorRelationship,
      classroomId: raw.selectedClassroomId
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
