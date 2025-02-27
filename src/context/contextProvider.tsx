import { createContext } from 'react';

export interface User {
    id?: string
    name?: string
}

export interface CombinedContextType {
    user: User
}

export const CombinedContext = createContext<CombinedContextType>({
    user: {}
})