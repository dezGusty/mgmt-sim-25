export enum RequestStatus {
  INVALID_REQUEST_STATUS = 0,
  ARRIVED = 1,
  PENDING = 2,
  APPROVED = 4,
  REJECTED = 8,
  EXPIRED = 16,
  CANCELED = 32,
}