export const runBackgrounProcess = (
  name: string,
  work: () => Promise<void>
) => {
  console.log(`starting background job: ${name}`);
  new Promise(async () => {
    while (true) { 
      try {
        await work();
      } catch (error) {
        console.error(`Background job ${name} error:`, error);
        await new Promise(resolve => setTimeout(resolve, 0));
      }
      finally
      {
        await new Promise(resolve => setTimeout(resolve, 0));
      }
    }
  });
  console.log(`background job ${name} is running`);
}