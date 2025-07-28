export interface JobTitleViewModel {
    id: number;
    name?: string;
    department?: {
        id: number;
        name?: string;
        description?: string;
    }
    employeeCount?: number;
}