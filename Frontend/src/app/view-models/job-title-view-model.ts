export interface IJobTitleViewModel {
    id: number;
    name?: string;
    department?: {
        id: number;
        name?: string;
        description?: string;
    }
    employeeCount?: number;
}