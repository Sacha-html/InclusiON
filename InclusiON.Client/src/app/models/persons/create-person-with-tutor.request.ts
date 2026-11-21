import { CreatePersonRequest } from './create-person.request';

export interface CreatePersonWithTutorRequest {
  student: CreatePersonRequest;
  existingTutorId?: string;
  tutorFirstName?: string;
  tutorLastName?: string;
  tutorEmail?: string;
  tutorDocumentNumber?: string;
  tutorPhone?: string;
  tutorRelationship: string;
  professionalId: string;
  classroomId?: string;
}
