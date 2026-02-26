
export enum StatusType {
    PENDING = 'PENDING',
    SENT = 'SENT',
    RETRIED = 'RETRIED',
    FAILED = 'FAILED'
}

export const statusTypes: StatusType[] = [
    StatusType.PENDING,
    StatusType.SENT,
    StatusType.RETRIED,
    StatusType.FAILED
]