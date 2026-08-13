import { CreatePersonRequest } from './create-person.request';

export interface CreatePersonWithTutorRequest {
  student: CreatePersonRequest;
  tutorFirstName: string;
  tutorLastName: string;
  tutorEmail: string;
  tutorDocumentNumber?: string;
  tutorPhone?: string;
  tutorRelationship: string;
  classroomId?: string;
}
