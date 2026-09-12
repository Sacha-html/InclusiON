import { buildProfessionalsCsv, getProfessionalStatusLabel } from './list.component';

describe('professional CSV export', () => {
  it('uses friendly columns, preserves the explicit order, and includes the UTF-8 BOM', () => {
    const csv = buildProfessionalsCsv([{
      id: 'internal-id',
      userId: 'internal-user-id',
      firstName: 'Ana',
      lastName: 'Gómez',
      fullName: 'Ana Gómez',
      documentNumber: '12,345',
      phone: '11 5555-1234',
      specialty: 'Educación Especial',
      specialtyId: 9,
      licenseNumber: 'MP-7',
      isActive: true,
      status: 'approved',
      email: 'ana@example.com',
    }]);

    expect(csv).toBe('\uFEFF"Nombre";"Apellido";"Documento";"Teléfono";"Especialidad";"Matrícula";"Email";"Estado"\r\n"Ana";"Gómez";"12,345";"11 5555-1234";"Educación Especial";"MP-7";"ana@example.com";"Aprobado"');
    expect(csv).not.toContain('internal-id');
    expect(csv).not.toContain('specialtyId');
  });

  it('escapes quotes, commas, newlines, and safely handles missing values', () => {
    const csv = buildProfessionalsCsv([{
      id: 'id',
      userId: 'user-id',
      firstName: 'Ana "A"',
      lastName: 'Gómez\nLópez',
      fullName: 'unused',
      isActive: false,
      status: 'pending',
    }]);

    expect(csv).toContain('"Ana ""A""";"Gómez\nLópez";"";"";"";"";"";"Pendiente"');
  });

  it('keeps unknown statuses readable and translates known local statuses', () => {
    expect(getProfessionalStatusLabel('terminated')).toBe('Dado de baja');
    expect(getProfessionalStatusLabel('Custom status')).toBe('Custom status');
    expect(getProfessionalStatusLabel(undefined)).toBe('');
  });
});
