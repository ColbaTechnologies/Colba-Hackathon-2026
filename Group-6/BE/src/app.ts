import express from 'express';
import router from './routes/messages.routes';
//import itemRoutes from './routes/itemRoutes';
//import { errorHandler } from './middlewares/errorHandler';

const app = express();

app.use(express.json());

app.use("/messages", router)
// Routes
//app.use('/api/items', itemRoutes);

// Global error handler (should be after routes)
//app.use(errorHandler);

export default app;