import express from 'express';
import cors from 'cors';
import messageRoutes from './routes/messages.routes';
//import { errorHandler } from './middlewares/errorHandler';

const app = express();

app.use(cors());
app.use(express.json());

app.use("/messages", router)
// Routes
app.use('/messages', messageRoutes);

// Global error handler (should be after routes)
//app.use(errorHandler);

export default app;