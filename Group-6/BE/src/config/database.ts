import { DocumentStore } from "ravendb";

const urls = ["http://localhost:8081"];
const database = "Group6";

const store = new DocumentStore(urls, database);
store.initialize();

export default store;