export const SPECIALTIES = [
  'Educación Especial',
  'Psicología',
  'Psicopedagogía',
  'Fonoaudiología',
  'Terapia Ocupacional',
  'Kinesiología',
  'Trabajo Social',
  'Musicoterapia',
  'Psicomotricidad',
  'Acompañamiento Terapéutico',
  'Docente de Apoyo a la Inclusión (DAI)',
  'Neurología',
  'Pediatría',
  'Otro',
] as const;

export type Specialty = (typeof SPECIALTIES)[number];
