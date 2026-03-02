import { DocumentStore } from "ravendb";

const urls = ["http://localhost:8080"];
const database = "Group6";

const store = new DocumentStore(urls, database);
store.initialize();

export default store;