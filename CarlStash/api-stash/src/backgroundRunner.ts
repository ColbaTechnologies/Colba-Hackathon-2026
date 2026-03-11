export const runBackgrounProcess = (work: () => Promise<void>) => new Promise(async () => {
  while (true) { 
    try {
      await work();
    } catch (error) {
      console.error('Background server error:', error);
      await new Promise(resolve => setTimeout(resolve, 1000));
    }
    console.log('Running backgroun server...')
  }
});