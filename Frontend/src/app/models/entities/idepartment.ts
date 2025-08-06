export interface IDepartment {
  id: number;
  name: string;
  description?: string;
  employeeCount? : number;
  deletedAt? : Date | null;
}