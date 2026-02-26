import { createRoute } from '@tanstack/react-router'
import { rootRoute } from './__root'
import Backoffice from '../pages/backoffice/backoffice'

export const backofficeRoute = createRoute({
    getParentRoute: () => rootRoute,
    path: '/backoffice',
    component: Backoffice,
})
