# Guía de Accesibilidad y Funcionamiento para InclusiON

## Basado en la Clasificación Internacional del Funcionamiento, de la Discapacidad y de la Salud (CIF - OMS)

Este documento proporciona lineamientos para implementar accesibilidad en el cliente Angular de InclusiON, siguiendo el marco conceptual de la CIF.

---

## 1. Marco Conceptual CIF

La CIF organiza el funcionamiento humano en dos partes principales que debemos considerar al diseñar interfaces accesibles:

### Parte 1: Funcionamiento y Discapacidad

| Componente | Descripción | Impacto en UI |
|------------|-------------|---------------|
| **Funciones Corporales (b)** | Funciones fisiológicas incluyendo psicológicas | Adaptaciones sensoriales, cognitivas |
| **Estructuras Corporales (s)** | Partes anatómicas del cuerpo | Adaptaciones para dispositivos de entrada |
| **Actividades y Participación (d)** | Tareas y acciones que realiza el individuo | Flujos de trabajo simplificados |

### Parte 2: Factores Contextuales

| Componente | Descripción | Impacto en UI |
|------------|-------------|---------------|
| **Factores Ambientales (e)** | Ambiente físico, social y actitudinal | Configuraciones de entorno |
| **Factores Personales** | Características individuales | Personalización del perfil |

---

## 2. Modelo de Datos para Perfiles Funcionales

### 2.1 Estructura de Códigos CIF

```typescript
// Prefijos de componentes CIF
export enum CIFComponentPrefix {
  BODY_FUNCTIONS = 'b',      // Funciones Corporales
  BODY_STRUCTURES = 's',     // Estructuras Corporales
  ACTIVITIES_PARTICIPATION = 'd', // Actividades y Participación
  ENVIRONMENTAL_FACTORS = 'e'     // Factores Ambientales
}

// Escala de calificadores CIF (0-4)
export enum CIFQualifierScale {
  NO_PROBLEM = 0,           // Sin problema (0-4%)
  MILD = 1,                 // Leve (5-24%)
  MODERATE = 2,             // Moderado (25-49%)
  SEVERE = 3,               // Grave (50-95%)
  COMPLETE = 4              // Completo (96-100%)
}

// Para factores ambientales (barreras vs facilitadores)
export enum EnvironmentalImpact {
  NO_BARRIER = 0,
  MILD_BARRIER = 1,
  MODERATE_BARRIER = 2,
  SEVERE_BARRIER = 3,
  COMPLETE_BARRIER = 4,
  NO_FACILITATOR = 0,       // Usar con signo +
  MILD_FACILITATOR = 1,
  MODERATE_FACILITATOR = 2,
  SUBSTANTIAL_FACILITATOR = 3,
  COMPLETE_FACILITATOR = 4
}
```

### 2.2 Interfaces para Perfil Funcional

```typescript
// Perfil funcional completo del usuario
export interface FunctionalProfile {
  id: string;
  userId: string;
  
  // Evaluaciones por componente CIF
  bodyFunctions: BodyFunctionAssessment[];
  bodyStructures: BodyStructureAssessment[];
  activitiesParticipation: ActivityParticipationAssessment[];
  environmentalFactors: EnvironmentalFactorAssessment[];
  
  // Configuración derivada de accesibilidad
  accessibilityConfig: AccessibilityConfiguration;
  
  // Metadata
  evaluatedAt: Date;
  evaluatedBy: string;
  nextReviewDate: Date;
}

// Evaluación de funciones corporales
export interface BodyFunctionAssessment {
  code: string;           // ej: "b210" (Funciones visuales)
  qualifier: CIFQualifierScale;
  description: string;
  notes?: string;
}

// Evaluación de actividades y participación
export interface ActivityParticipationAssessment {
  code: string;           // ej: "d330" (Hablar)
  performanceQualifier: CIFQualifierScale;  // Desempeño en entorno real
  capacityQualifier: CIFQualifierScale;     // Capacidad en entorno uniforme
  description: string;
  notes?: string;
}

// Evaluación de factores ambientales
export interface EnvironmentalFactorAssessment {
  code: string;           // ej: "e125" (Productos para comunicación)
  impact: EnvironmentalImpact;
  isBarrier: boolean;     // true = barrera, false = facilitador
  description: string;
}
```

---

## 3. Dominios CIF Relevantes para Accesibilidad UI

### 3.1 Funciones Corporales (Capítulos b1-b8)

#### b1 - Funciones Mentales

```typescript
export interface MentalFunctionConfig {
  // b110-b139 Funciones mentales globales
  consciousness: {
    alertnessLevel: CIFQualifierScale;
    // Afecta: tiempos de respuesta, complejidad de interacciones
  };
  
  // b140-b189 Funciones mentales específicas
  attention: {
    sustainedAttention: CIFQualifierScale;
    dividedAttention: CIFQualifierScale;
    // Afecta: cantidad de elementos en pantalla, distracciones
  };
  
  memory: {
    shortTermMemory: CIFQualifierScale;
    longTermMemory: CIFQualifierScale;
    // Afecta: persistencia de información, recordatorios
  };
  
  languageFunctions: {
    reception: CIFQualifierScale;    // b1670 Recepción del lenguaje
    expression: CIFQualifierScale;   // b1671 Expresión del lenguaje
    // Afecta: uso de SAAC, pictogramas
  };
  
  calculationFunctions: CIFQualifierScale;  // b172
  // Afecta: presentación de datos numéricos
}
```

**Adaptaciones UI según funciones mentales:**

```typescript
export function getUIAdaptationsForMentalFunctions(
  config: MentalFunctionConfig
): UIAdaptations {
  return {
    // Atención
    maxElementsPerScreen: config.attention.sustainedAttention >= 2 ? 3 : 6,
    showProgressIndicators: config.attention.sustainedAttention >= 1,
    minimizeAnimations: config.attention.dividedAttention >= 2,
    
    // Memoria
    showPersistentBreadcrumbs: config.memory.shortTermMemory >= 2,
    enableAutoSave: config.memory.shortTermMemory >= 1,
    showContextualReminders: config.memory.shortTermMemory >= 2,
    
    // Lenguaje
    usePictograms: config.languageFunctions.reception >= 2,
    enableTextToSpeech: config.languageFunctions.reception >= 2,
    simplifyTextLevel: mapQualifierToReadingLevel(config.languageFunctions.reception),
    
    // Cálculo
    useVisualNumbers: config.calculationFunctions >= 2,
    showCalculationAids: config.calculationFunctions >= 2
  };
}
```

#### b2 - Funciones Sensoriales y Dolor

```typescript
export interface SensoryFunctionConfig {
  // b210 Funciones visuales
  vision: {
    acuity: CIFQualifierScale;
    fieldOfVision: CIFQualifierScale;
    colorVision: CIFQualifierScale;
    contrastSensitivity: CIFQualifierScale;  // b21022
  };
  
  // b230 Funciones auditivas
  hearing: {
    detection: CIFQualifierScale;
    discrimination: CIFQualifierScale;
  };
  
  // b250 Función gustativa, b255 Función olfativa (menos relevante para UI)
  
  // b260 Función propioceptiva
  proprioception: CIFQualifierScale;
  
  // b265 Función táctil
  touch: CIFQualifierScale;
}
```

**Adaptaciones UI según funciones sensoriales:**

```typescript
export function getUIAdaptationsForSensoryFunctions(
  config: SensoryFunctionConfig
): UIAdaptations {
  return {
    // Visión
    fontSize: calculateFontSize(config.vision.acuity),
    highContrast: config.vision.contrastSensitivity >= 2,
    colorBlindMode: config.vision.colorVision >= 1 ? detectColorBlindType() : null,
    enlargedTouchTargets: config.vision.acuity >= 2,
    screenReaderOptimized: config.vision.acuity >= 3,
    
    // Audición
    visualAlerts: config.hearing.detection >= 2,
    captionsEnabled: config.hearing.detection >= 1,
    vibrationFeedback: config.hearing.detection >= 2,
    
    // Táctil/Propioceptivo
    hapticFeedbackIntensity: calculateHapticIntensity(config.touch),
    largerInteractionAreas: config.proprioception >= 2
  };
}
```

#### b7 - Funciones Neuromusculoesqueléticas y Relacionadas con el Movimiento

```typescript
export interface MotorFunctionConfig {
  // b710-b729 Funciones de las articulaciones y huesos
  jointMobility: CIFQualifierScale;
  
  // b730-b749 Funciones musculares
  muscleStrength: CIFQualifierScale;
  muscleTone: CIFQualifierScale;
  
  // b750-b789 Funciones relacionadas con el movimiento
  motorReflex: CIFQualifierScale;
  voluntaryMovementControl: CIFQualifierScale;  // b760
  involuntaryMovements: CIFQualifierScale;       // b765 (temblores, tics)
  coordinationOfVoluntaryMovements: CIFQualifierScale;  // b7601-b7602
}
```

**Adaptaciones UI según funciones motoras:**

```typescript
export function getUIAdaptationsForMotorFunctions(
  config: MotorFunctionConfig
): UIAdaptations {
  return {
    // Control de movimiento
    touchTargetSize: calculateTouchTargetSize(config.voluntaryMovementControl),
    dragAndDropEnabled: config.coordinationOfVoluntaryMovements < 2,
    swipeGesturesEnabled: config.coordinationOfVoluntaryMovements < 2,
    
    // Temblores/movimientos involuntarios
    clickDelayMs: config.involuntaryMovements >= 2 ? 500 : 0,
    doubleClickProtection: config.involuntaryMovements >= 2,
    touchDebounceMs: config.involuntaryMovements >= 1 ? 300 : 100,
    
    // Fuerza y movilidad
    switchAccessEnabled: config.muscleStrength >= 3 || config.jointMobility >= 3,
    voiceControlEnabled: config.muscleStrength >= 3,
    eyeTrackingEnabled: config.muscleStrength >= 4,
    
    // Coordinación general
    simplifiedGestures: config.coordinationOfVoluntaryMovements >= 2,
    singleTapOnly: config.coordinationOfVoluntaryMovements >= 3
  };
}
```

---

### 3.2 Actividades y Participación (Capítulos d1-d9)

Los calificadores de este componente son fundamentales:

- **Desempeño/Realización**: Lo que hace en su entorno real
- **Capacidad**: Lo que puede hacer en un entorno estandarizado

```typescript
export interface ActivityParticipationConfig {
  // d1 Aprendizaje y aplicación del conocimiento
  learning: {
    watching: CIFQualifierScale;           // d110
    listening: CIFQualifierScale;          // d115
    copying: CIFQualifierScale;            // d130
    learning: CIFQualifierScale;           // d140-d159
    focusing: CIFQualifierScale;           // d160
    thinking: CIFQualifierScale;           // d163
    reading: CIFQualifierScale;            // d166
    writing: CIFQualifierScale;            // d170
    calculating: CIFQualifierScale;        // d172
    solvingProblems: CIFQualifierScale;    // d175
    makingDecisions: CIFQualifierScale;    // d177
  };
  
  // d2 Tareas y demandas generales
  tasks: {
    undertakingSingleTask: CIFQualifierScale;    // d210
    undertakingMultipleTasks: CIFQualifierScale; // d220
    carryingOutDailyRoutine: CIFQualifierScale;  // d230
    handlingStress: CIFQualifierScale;           // d240
  };
  
  // d3 Comunicación
  communication: {
    receivingSpokenMessages: CIFQualifierScale;  // d310
    receivingNonverbal: CIFQualifierScale;       // d315
    receivingWritten: CIFQualifierScale;         // d325
    speaking: CIFQualifierScale;                 // d330
    producingNonverbal: CIFQualifierScale;       // d335
    writing: CIFQualifierScale;                  // d345
    conversation: CIFQualifierScale;             // d350
    usingDevices: CIFQualifierScale;             // d360
  };
  
  // d4 Movilidad (relevante para dispositivos adaptativos)
  mobility: {
    fineHandUse: CIFQualifierScale;      // d440
    handAndArmUse: CIFQualifierScale;    // d445
  };
}
```

---

## 4. Configuración de Accesibilidad Derivada

### 4.1 Interface Principal de Configuración

```typescript
export interface AccessibilityConfiguration {
  // Configuración visual
  visual: {
    theme: 'light' | 'dark' | 'high-contrast' | 'custom';
    fontSize: 'small' | 'medium' | 'large' | 'extra-large';
    fontFamily: string;
    lineHeight: number;
    letterSpacing: number;
    colorScheme: ColorScheme;
    reduceMotion: boolean;
    reduceTransparency: boolean;
  };
  
  // Configuración auditiva
  auditory: {
    textToSpeechEnabled: boolean;
    textToSpeechRate: number;
    textToSpeechVoice: string;
    soundEffectsEnabled: boolean;
    visualAlertsEnabled: boolean;
    captionsEnabled: boolean;
  };
  
  // Configuración motora
  motor: {
    touchTargetSize: 'default' | 'large' | 'extra-large';
    clickDelay: number;
    longPressDelay: number;
    swipeEnabled: boolean;
    dragDropEnabled: boolean;
    keyboardNavigationOnly: boolean;
    switchAccessEnabled: boolean;
    dwellClickEnabled: boolean;
    dwellClickDelay: number;
  };
  
  // Configuración cognitiva
  cognitive: {
    simplifiedInterface: boolean;
    readingLevel: 'basic' | 'intermediate' | 'advanced';
    pictogramsEnabled: boolean;
    pictogramSystem: 'arasaac' | 'soyvisual' | 'buhobo' | 'custom';
    maxOptionsPerScreen: number;
    showProgressIndicators: boolean;
    enableReminders: boolean;
    autoSaveInterval: number;
    confirmBeforeActions: boolean;
    undoEnabled: boolean;
  };
  
  // Dispositivos adaptativos
  adaptiveDevices: {
    switchAccess: SwitchAccessConfig | null;
    eyeTracking: EyeTrackingConfig | null;
    voiceControl: VoiceControlConfig | null;
    adaptedMouse: AdaptedMouseConfig | null;
  };
  
  // SAAC (Sistemas Aumentativos y Alternativos de Comunicación)
  saac: SAACConfiguration;
}

export interface SAACConfiguration {
  enabled: boolean;
  primarySystem: 'arasaac' | 'soyvisual' | 'buhobo' | 'pecs' | 'custom';
  pictogramSize: 'small' | 'medium' | 'large';
  showTextWithPictogram: boolean;
  textPosition: 'above' | 'below' | 'none';
  gridColumns: number;
  gridRows: number;
  categoryNavigation: boolean;
  recentPictogramsEnabled: boolean;
  customPictograms: CustomPictogram[];
}
```

---

## 5. Componentes Angular Accesibles

### 5.1 Servicio de Accesibilidad

```typescript
// accessibility.service.ts
@Injectable({
  providedIn: 'root'
})
export class AccessibilityService {
  private config$ = new BehaviorSubject<AccessibilityConfiguration>(DEFAULT_CONFIG);
  
  // Genera configuración basada en perfil CIF
  generateConfigFromProfile(profile: FunctionalProfile): AccessibilityConfiguration {
    const config = { ...DEFAULT_CONFIG };
    
    // Procesar funciones corporales
    this.applyBodyFunctionAdaptations(config, profile.bodyFunctions);
    
    // Procesar actividades y participación
    this.applyActivityAdaptations(config, profile.activitiesParticipation);
    
    // Procesar factores ambientales (facilitadores)
    this.applyEnvironmentalFacilitators(config, profile.environmentalFactors);
    
    return config;
  }
  
  private applyBodyFunctionAdaptations(
    config: AccessibilityConfiguration, 
    assessments: BodyFunctionAssessment[]
  ): void {
    // b210 - Funciones visuales
    const visualFunction = assessments.find(a => a.code.startsWith('b210'));
    if (visualFunction && visualFunction.qualifier >= 2) {
      config.visual.fontSize = visualFunction.qualifier >= 3 ? 'extra-large' : 'large';
      config.visual.highContrast = true;
      config.auditory.textToSpeechEnabled = visualFunction.qualifier >= 3;
    }
    
    // b140 - Funciones de la atención
    const attentionFunction = assessments.find(a => a.code.startsWith('b140'));
    if (attentionFunction && attentionFunction.qualifier >= 2) {
      config.cognitive.simplifiedInterface = true;
      config.cognitive.maxOptionsPerScreen = 3;
      config.visual.reduceMotion = true;
    }
    
    // b760 - Control de movimientos voluntarios
    const motorControl = assessments.find(a => a.code.startsWith('b760'));
    if (motorControl && motorControl.qualifier >= 2) {
      config.motor.touchTargetSize = 'extra-large';
      config.motor.clickDelay = 300;
      config.motor.swipeEnabled = false;
    }
    
    // ... más adaptaciones
  }
}
```

### 5.2 Directiva de Accesibilidad

```typescript
// accessible-element.directive.ts
@Directive({
  selector: '[appAccessible]'
})
export class AccessibleElementDirective implements OnInit {
  @Input() accessibleRole: string;
  @Input() accessibleLabel: string;
  @Input() accessibleHint: string;
  
  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
    private accessibilityService: AccessibilityService
  ) {}
  
  ngOnInit(): void {
    this.accessibilityService.config$.subscribe(config => {
      this.applyAccessibilityConfig(config);
    });
  }
  
  private applyAccessibilityConfig(config: AccessibilityConfiguration): void {
    const element = this.el.nativeElement;
    
    // Aplicar tamaño de target táctil
    if (config.motor.touchTargetSize === 'extra-large') {
      this.renderer.setStyle(element, 'min-height', '56px');
      this.renderer.setStyle(element, 'min-width', '56px');
      this.renderer.setStyle(element, 'padding', '16px');
    }
    
    // ARIA attributes
    if (this.accessibleRole) {
      this.renderer.setAttribute(element, 'role', this.accessibleRole);
    }
    if (this.accessibleLabel) {
      this.renderer.setAttribute(element, 'aria-label', this.accessibleLabel);
    }
    
    // Pictogramas
    if (config.cognitive.pictogramsEnabled && this.accessibleLabel) {
      this.addPictogram(this.accessibleLabel, config.saac);
    }
  }
}
```

### 5.3 Componente de Botón Accesible

```typescript
// accessible-button.component.ts
@Component({
  selector: 'app-accessible-button',
  template: `
    <button
      [class]="buttonClasses"
      [style]="buttonStyles"
      [attr.aria-label]="ariaLabel"
      [attr.aria-describedby]="ariaDescribedBy"
      (click)="handleClick($event)"
      (keydown)="handleKeydown($event)"
    >
      <!-- Pictograma si está habilitado -->
      <ng-container *ngIf="showPictogram && pictogramUrl">
        <img 
          [src]="pictogramUrl" 
          [alt]="pictogramAlt"
          class="pictogram"
          [style.width.px]="pictogramSize"
          [style.height.px]="pictogramSize"
        />
      </ng-container>
      
      <!-- Texto del botón -->
      <span 
        class="button-text"
        [class.visually-hidden]="hideText"
      >
        <ng-content></ng-content>
      </span>
      
      <!-- Indicador de carga -->
      <span *ngIf="loading" class="loading-indicator" aria-hidden="true">
        <span class="spinner"></span>
      </span>
    </button>
  `,
  styles: [`
    :host {
      display: inline-block;
    }
    
    button {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      transition: background-color 0.2s, transform 0.1s;
      font-family: inherit;
    }
    
    /* Tamaños según configuración de accesibilidad */
    button.size-default {
      min-height: 44px;
      min-width: 44px;
      padding: 12px 24px;
      font-size: 1rem;
    }
    
    button.size-large {
      min-height: 56px;
      min-width: 56px;
      padding: 16px 32px;
      font-size: 1.25rem;
    }
    
    button.size-extra-large {
      min-height: 72px;
      min-width: 72px;
      padding: 20px 40px;
      font-size: 1.5rem;
    }
    
    /* Alto contraste */
    button.high-contrast {
      border: 3px solid currentColor;
    }
    
    /* Focus visible */
    button:focus-visible {
      outline: 4px solid var(--focus-color, #005fcc);
      outline-offset: 2px;
    }
    
    /* Pictograma */
    .pictogram {
      flex-shrink: 0;
    }
    
    .visually-hidden {
      position: absolute;
      width: 1px;
      height: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      white-space: nowrap;
      border: 0;
    }
  `]
})
export class AccessibleButtonComponent implements OnInit {
  @Input() label: string;
  @Input() pictogramCode: string;
  @Input() confirmAction = false;
  @Input() loading = false;
  @Output() buttonClick = new EventEmitter<void>();
  
  config: AccessibilityConfiguration;
  pictogramUrl: string;
  
  constructor(
    private accessibilityService: AccessibilityService,
    private pictogramService: PictogramService
  ) {}
  
  ngOnInit(): void {
    this.accessibilityService.config$.subscribe(config => {
      this.config = config;
      if (config.cognitive.pictogramsEnabled && this.pictogramCode) {
        this.loadPictogram();
      }
    });
  }
  
  get showPictogram(): boolean {
    return this.config?.cognitive?.pictogramsEnabled && !!this.pictogramCode;
  }
  
  get buttonClasses(): string {
    const classes = ['accessible-button'];
    classes.push(`size-${this.config?.motor?.touchTargetSize || 'default'}`);
    if (this.config?.visual?.theme === 'high-contrast') {
      classes.push('high-contrast');
    }
    return classes.join(' ');
  }
  
  async handleClick(event: Event): Promise<void> {
    if (this.config?.cognitive?.confirmBeforeActions && this.confirmAction) {
      const confirmed = await this.showConfirmation();
      if (!confirmed) return;
    }
    this.buttonClick.emit();
  }
}
```

---

## 6. Principios de Lectura Fácil

### 6.1 Directrices de Texto

```typescript
export interface EasyReadingConfig {
  // Principio: Una idea por oración
  maxWordsPerSentence: number;     // Máximo 15-20 palabras
  
  // Principio: Vocabulario simple
  vocabularyLevel: 'basic' | 'intermediate';
  avoidAbstractConcepts: boolean;
  useConcreteExamples: boolean;
  
  // Principio: Estructura clara
  useShortParagraphs: boolean;     // Máximo 3-4 oraciones
  useVisualBreaks: boolean;
  useBulletPoints: boolean;
  
  // Principio: Apoyo visual
  accompanyWithImages: boolean;
  useIconsForActions: boolean;
  highlightKeyWords: boolean;
}

// Servicio de transformación de texto
@Injectable()
export class EasyReadingService {
  
  transformText(text: string, config: EasyReadingConfig): TransformedText {
    return {
      original: text,
      simplified: this.simplifyText(text, config),
      withPictograms: this.addPictogramSupport(text),
      audioVersion: this.generateAudioUrl(text)
    };
  }
  
  private simplifyText(text: string, config: EasyReadingConfig): string {
    // Implementar simplificación según nivel
    // - Dividir oraciones largas
    // - Reemplazar palabras complejas
    // - Agregar explicaciones a términos técnicos
    return text; // Placeholder
  }
}
```

### 6.2 Componente de Texto Accesible

```typescript
// accessible-text.component.ts
@Component({
  selector: 'app-accessible-text',
  template: `
    <div class="accessible-text" [class]="textClasses">
      <!-- Versión con pictogramas -->
      <div *ngIf="showPictogramsVersion" class="pictogram-version">
        <ng-container *ngFor="let segment of textSegments">
          <span class="text-segment">
            <img 
              *ngIf="segment.pictogram" 
              [src]="segment.pictogram" 
              [alt]="segment.text"
              class="inline-pictogram"
            />
            <span [innerHTML]="segment.text"></span>
          </span>
        </ng-container>
      </div>
      
      <!-- Versión texto simple -->
      <div *ngIf="!showPictogramsVersion" class="text-version">
        <p [innerHTML]="displayText"></p>
      </div>
      
      <!-- Botón de audio -->
      <button 
        *ngIf="showAudioButton"
        class="audio-button"
        (click)="toggleAudio()"
        [attr.aria-label]="isPlaying ? 'Detener lectura' : 'Escuchar texto'"
      >
        <span class="audio-icon" [class.playing]="isPlaying"></span>
      </button>
    </div>
  `
})
export class AccessibleTextComponent {
  @Input() text: string;
  @Input() level: 'heading' | 'body' | 'label' = 'body';
  @Input() importance: 'primary' | 'secondary' = 'primary';
  
  // Procesamiento según configuración de accesibilidad
}
```

---

## 7. Principio de Una Acción por Instrucción

### 7.1 Diseño de Flujos

```typescript
// Componente de paso único
@Component({
  selector: 'app-single-action-step',
  template: `
    <div class="single-action-container">
      <!-- Instrucción clara -->
      <app-accessible-text 
        [text]="instruction"
        level="heading"
        importance="primary"
      ></app-accessible-text>
      
      <!-- Pictograma de la acción -->
      <div class="action-pictogram" *ngIf="actionPictogram">
        <img [src]="actionPictogram" [alt]="instruction" />
      </div>
      
      <!-- Área de acción única -->
      <div class="action-area">
        <ng-content></ng-content>
      </div>
      
      <!-- Feedback visual -->
      <div class="feedback-area" *ngIf="showFeedback">
        <app-feedback-indicator [state]="feedbackState"></app-feedback-indicator>
      </div>
      
      <!-- Navegación simplificada -->
      <div class="navigation">
        <app-accessible-button
          *ngIf="showBack"
          (buttonClick)="goBack()"
          pictogramCode="arrow-left"
        >
          Atrás
        </app-accessible-button>
        
        <app-accessible-button
          *ngIf="showNext"
          (buttonClick)="goNext()"
          pictogramCode="arrow-right"
          [confirmAction]="confirmNext"
        >
          Siguiente
        </app-accessible-button>
      </div>
    </div>
  `
})
export class SingleActionStepComponent {
  @Input() instruction: string;
  @Input() actionPictogram: string;
  @Input() showBack = true;
  @Input() showNext = true;
  @Input() confirmNext = false;
  
  @Output() next = new EventEmitter<void>();
  @Output() back = new EventEmitter<void>();
}
```

### 7.2 Wizard de Microtareas

```typescript
// micro-task-wizard.component.ts
@Component({
  selector: 'app-micro-task-wizard',
  template: `
    <div class="wizard-container">
      <!-- Indicador de progreso visual -->
      <app-visual-progress
        [totalSteps]="steps.length"
        [currentStep]="currentStepIndex"
        [completedSteps]="completedSteps"
      ></app-visual-progress>
      
      <!-- Paso actual -->
      <app-single-action-step
        [instruction]="currentStep.instruction"
        [actionPictogram]="currentStep.pictogram"
        [showBack]="currentStepIndex > 0"
        [showNext]="!isLastStep"
        (back)="previousStep()"
        (next)="nextStep()"
      >
        <!-- Contenido dinámico del paso -->
        <ng-container [ngSwitch]="currentStep.type">
          <app-selection-step 
            *ngSwitchCase="'selection'"
            [options]="currentStep.options"
            (selected)="onStepComplete($event)"
          ></app-selection-step>
          
          <app-input-step
            *ngSwitchCase="'input'"
            [inputType]="currentStep.inputType"
            (submitted)="onStepComplete($event)"
          ></app-input-step>
          
          <app-confirmation-step
            *ngSwitchCase="'confirmation'"
            [data]="collectedData"
            (confirmed)="onWizardComplete()"
          ></app-confirmation-step>
        </ng-container>
      </app-single-action-step>
      
      <!-- Celebración al completar -->
      <app-celebration-feedback
        *ngIf="showCelebration"
        [message]="celebrationMessage"
      ></app-celebration-feedback>
    </div>
  `
})
export class MicroTaskWizardComponent {
  @Input() steps: MicroTaskStep[];
  @Output() completed = new EventEmitter<any>();
  
  currentStepIndex = 0;
  completedSteps: number[] = [];
  collectedData: any = {};
}

export interface MicroTaskStep {
  id: string;
  instruction: string;
  pictogram: string;
  type: 'selection' | 'input' | 'confirmation' | 'action';
  options?: StepOption[];
  inputType?: 'text' | 'number' | 'pictogram-selection';
  validation?: (value: any) => boolean;
}
```

---

## 8. Integración con SAAC

### 8.1 Servicio de Pictogramas ARASAAC

```typescript
// pictogram.service.ts
@Injectable({
  providedIn: 'root'
})
export class PictogramService {
  private readonly ARASAAC_API = 'https://api.arasaac.org/api';
  
  // Obtener pictograma por palabra clave
  async getPictogramByKeyword(
    keyword: string, 
    options: PictogramOptions = {}
  ): Promise<Pictogram[]> {
    const params = new URLSearchParams({
      locale: options.locale || 'es',
      ...options
    });
    
    const response = await fetch(
      `${this.ARASAAC_API}/pictograms/search/${encodeURIComponent(keyword)}?${params}`
    );
    
    return response.json();
  }
  
  // Obtener URL de pictograma
  getPictogramUrl(id: number, options: PictogramRenderOptions = {}): string {
    const params = new URLSearchParams();
    
    if (options.color) params.set('color', 'true');
    if (options.skin) params.set('skin', options.skin);
    if (options.hair) params.set('hair', options.hair);
    
    return `${this.ARASAAC_API}/pictograms/${id}?${params}`;
  }
  
  // Caché local de pictogramas frecuentes
  async preloadCommonPictograms(): Promise<void> {
    const commonWords = [
      'hola', 'adios', 'si', 'no', 'ayuda', 'bien', 'mal',
      'comer', 'beber', 'dormir', 'jugar', 'trabajar',
      'feliz', 'triste', 'enfadado', 'cansado'
    ];
    
    // Precargar en IndexedDB o caché
  }
}

export interface PictogramOptions {
  locale?: string;
  plural?: boolean;
  verbTime?: 'present' | 'past' | 'future';
}

export interface PictogramRenderOptions {
  color?: boolean;
  skin?: 'white' | 'black' | 'assian' | 'mulatto' | 'aztec';
  hair?: 'blonde' | 'brown' | 'darkBrown' | 'gray' | 'darkGray' | 'red' | 'black';
  backgroundColor?: string;
}
```

### 8.2 Componente de Tablero de Comunicación

```typescript
// communication-board.component.ts
@Component({
  selector: 'app-communication-board',
  template: `
    <div class="communication-board" [style.gridTemplateColumns]="gridColumns">
      <!-- Categorías -->
      <nav class="categories" *ngIf="showCategories" role="navigation">
        <button
          *ngFor="let category of categories"
          [class.active]="selectedCategory === category.id"
          (click)="selectCategory(category.id)"
        >
          <img [src]="category.pictogram" [alt]="category.name" />
          <span>{{ category.name }}</span>
        </button>
      </nav>
      
      <!-- Grid de pictogramas -->
      <div class="pictogram-grid" role="grid">
        <button
          *ngFor="let pictogram of visiblePictograms"
          class="pictogram-cell"
          (click)="selectPictogram(pictogram)"
          [attr.aria-label]="pictogram.label"
        >
          <img 
            [src]="pictogram.url" 
            [alt]="pictogram.label"
            loading="lazy"
          />
          <span *ngIf="showLabels" class="pictogram-label">
            {{ pictogram.label }}
          </span>
        </button>
      </div>
      
      <!-- Área de mensaje construido -->
      <div class="message-area" role="region" aria-live="polite">
        <div class="selected-pictograms">
          <img 
            *ngFor="let p of selectedPictograms; let i = index"
            [src]="p.url"
            [alt]="p.label"
            (click)="removePictogram(i)"
          />
        </div>
        
        <div class="message-actions">
          <app-accessible-button
            (buttonClick)="speakMessage()"
            pictogramCode="speaker"
          >
            Hablar
          </app-accessible-button>
          
          <app-accessible-button
            (buttonClick)="clearMessage()"
            pictogramCode="delete"
          >
            Borrar
          </app-accessible-button>
        </div>
      </div>
    </div>
  `
})
export class CommunicationBoardComponent {
  @Input() config: SAACConfiguration;
  @Input() categories: PictogramCategory[];
  @Output() messageCreated = new EventEmitter<Pictogram[]>();
  
  selectedPictograms: Pictogram[] = [];
  
  // Síntesis de voz para el mensaje
  speakMessage(): void {
    const message = this.selectedPictograms
      .map(p => p.label)
      .join(' ');
    
    const utterance = new SpeechSynthesisUtterance(message);
    utterance.lang = 'es-ES';
    speechSynthesis.speak(utterance);
  }
}
```

---

## 9. Factores Ambientales como Facilitadores

### 9.1 Productos y Tecnología de Apoyo (e1)

```typescript
// Configuración de tecnologías de apoyo
export interface AssistiveTechnologyConfig {
  // e1150 Productos para uso personal en la vida diaria
  personalDevices: {
    switchDevice: SwitchDeviceConfig | null;
    adaptedMouse: AdaptedMouseConfig | null;
    touchScreen: TouchScreenConfig | null;
  };
  
  // e1251 Productos de apoyo para la comunicación
  communicationDevices: {
    aacDevice: boolean;
    textToSpeech: TextToSpeechConfig;
    speechToText: SpeechToTextConfig;
  };
  
  // e1300 Productos para la educación
  educationalProducts: {
    specializedSoftware: string[];
    adaptedMaterials: boolean;
  };
}

// Servicio de integración con dispositivos adaptativos
@Injectable()
export class AdaptiveDeviceService {
  
  // Detectar dispositivos conectados
  async detectDevices(): Promise<ConnectedDevice[]> {
    const devices: ConnectedDevice[] = [];
    
    // Detectar switches USB/Bluetooth
    if ('hid' in navigator) {
      const hidDevices = await (navigator as any).hid.getDevices();
      devices.push(...this.parseSwitchDevices(hidDevices));
    }
    
    // Detectar gamepads (pueden usarse como switches)
    const gamepads = navigator.getGamepads();
    devices.push(...this.parseGamepadAsSwitches(gamepads));
    
    return devices;
  }
  
  // Configurar switch scanning
  setupSwitchScanning(config: SwitchScanningConfig): void {
    // Implementar escaneo automático o manual
    // con tiempos configurables según perfil CIF
  }
}
```

---

## 10. Estilos CSS Accesibles

### 10.1 Variables CSS Base

```scss
// _accessibility-variables.scss
:root {
  // Tamaños de fuente según configuración
  --font-size-small: 14px;
  --font-size-medium: 16px;
  --font-size-large: 20px;
  --font-size-extra-large: 24px;
  
  // Tamaños de target táctil (WCAG 2.2)
  --touch-target-default: 44px;
  --touch-target-large: 56px;
  --touch-target-extra-large: 72px;
  
  // Espaciado
  --spacing-tight: 8px;
  --spacing-normal: 16px;
  --spacing-relaxed: 24px;
  --spacing-loose: 32px;
  
  // Colores de alto contraste
  --hc-background: #000000;
  --hc-foreground: #ffffff;
  --hc-primary: #ffff00;
  --hc-secondary: #00ffff;
  --hc-error: #ff6b6b;
  --hc-success: #51cf66;
  
  // Focus visible
  --focus-ring-width: 3px;
  --focus-ring-offset: 2px;
  --focus-ring-color: #005fcc;
  
  // Transiciones (respetando prefers-reduced-motion)
  --transition-duration: 200ms;
}

// Modo de movimiento reducido
@media (prefers-reduced-motion: reduce) {
  :root {
    --transition-duration: 0ms;
  }
  
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}

// Alto contraste
@media (prefers-contrast: more) {
  :root {
    --border-width: 2px;
  }
}

// Tema oscuro
@media (prefers-color-scheme: dark) {
  :root {
    --background-primary: #1a1a1a;
    --text-primary: #ffffff;
  }
}
```

### 10.2 Mixins de Accesibilidad

```scss
// _accessibility-mixins.scss

// Mixin para elementos interactivos accesibles
@mixin accessible-interactive {
  // Tamaño mínimo de target
  min-height: var(--touch-target-default);
  min-width: var(--touch-target-default);
  
  // Cursor
  cursor: pointer;
  
  // Focus visible
  &:focus-visible {
    outline: var(--focus-ring-width) solid var(--focus-ring-color);
    outline-offset: var(--focus-ring-offset);
  }
  
  // Estados
  &:hover:not(:disabled) {
    opacity: 0.9;
  }
  
  &:active:not(:disabled) {
    transform: scale(0.98);
  }
  
  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
}

// Mixin para texto legible
@mixin readable-text($level: 'body') {
  @if $level == 'heading' {
    font-size: var(--font-size-large);
    font-weight: 600;
    line-height: 1.3;
  } @else {
    font-size: var(--font-size-medium);
    font-weight: 400;
    line-height: 1.6;
  }
  
  letter-spacing: 0.01em;
  word-spacing: 0.05em;
}

// Mixin para ocultar visualmente pero mantener accesible
@mixin visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

// Mixin para alto contraste
@mixin high-contrast-element {
  border: 2px solid currentColor;
  background-color: var(--hc-background);
  color: var(--hc-foreground);
  
  &:focus-visible {
    outline-color: var(--hc-primary);
  }
}
```

---

## 11. Testing de Accesibilidad

### 11.1 Tests Automatizados

```typescript
// accessibility.spec.ts
describe('Accessibility Tests', () => {
  
  it('should have sufficient color contrast', async () => {
    const results = await axe.run(document, {
      rules: ['color-contrast']
    });
    expect(results.violations).toHaveLength(0);
  });
  
  it('should have proper ARIA labels', async () => {
    const buttons = document.querySelectorAll('button');
    buttons.forEach(button => {
      const hasLabel = button.getAttribute('aria-label') || 
                       button.textContent?.trim();
      expect(hasLabel).toBeTruthy();
    });
  });
  
  it('should respect touch target minimum size', () => {
    const interactiveElements = document.querySelectorAll(
      'button, a, input, [role="button"]'
    );
    
    interactiveElements.forEach(element => {
      const rect = element.getBoundingClientRect();
      expect(rect.width).toBeGreaterThanOrEqual(44);
      expect(rect.height).toBeGreaterThanOrEqual(44);
    });
  });
  
  it('should support keyboard navigation', () => {
    const focusableElements = document.querySelectorAll(
      'button, a, input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    
    expect(focusableElements.length).toBeGreaterThan(0);
    
    // Verificar orden lógico de tabulación
    const tabOrder = Array.from(focusableElements)
      .map(el => el.tabIndex)
      .filter(index => index >= 0);
    
    // No debe haber saltos grandes en tabindex
    expect(Math.max(...tabOrder)).toBeLessThan(100);
  });
});
```

---

## 12. Checklist de Implementación

### Funciones Corporales (b)
- [ ] b1 Mental: Configuración de complejidad de interfaz, tiempos de respuesta
- [ ] b2 Sensorial: Ajustes visuales, auditivos, táctiles
- [ ] b7 Motor: Tamaño de targets, gestos alternativos, switch access

### Actividades y Participación (d)
- [ ] d1 Aprendizaje: Lectura fácil, pictogramas, audio
- [ ] d2 Tareas: Una acción por instrucción, microtareas
- [ ] d3 Comunicación: SAAC, tableros de comunicación
- [ ] d4 Movilidad: Dispositivos adaptativos

### Factores Ambientales (e)
- [ ] e1 Productos: Integración con tecnologías de apoyo
- [ ] e3 Apoyo: Configuración para acompañantes/profesionales
- [ ] e5 Servicios: Integración con servicios de accesibilidad

### WCAG 2.2 AA
- [ ] Perceptible: Alternativas de texto, contraste, redimensionamiento
- [ ] Operable: Teclado, tiempo suficiente, navegación
- [ ] Comprensible: Legible, predecible, asistencia de entrada
- [ ] Robusto: Compatible con tecnologías de apoyo

---

## Referencias

- OMS (2001). Clasificación Internacional del Funcionamiento, de la Discapacidad y de la Salud (CIF)
- WCAG 2.2 - Web Content Accessibility Guidelines
- ARASAAC - Portal Aragonés de la Comunicación Aumentativa y Alternativa
- Lectura Fácil - Directrices IFLA/Inclusion Europe
