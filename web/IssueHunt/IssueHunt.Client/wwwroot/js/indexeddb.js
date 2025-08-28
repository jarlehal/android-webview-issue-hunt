export function createLaudIndexedDb(databaseName, storeName, version) {
    return new LaudIndexedDb(databaseName, storeName, version);
}

export class LaudIndexedDb {
    #databaseName
    #storeName
    #version
    
    constructor(databaseName, storeName, version) {
        this.#databaseName = databaseName;
        this.#storeName = storeName;
        this.#version = version;
    }
    
    initialize() {
        let dbOpenRequest = indexedDB.open(this.#databaseName, this.#version)
        
        const storeName = this.#storeName
        dbOpenRequest.onupgradeneeded = function ()
        {
            let db = dbOpenRequest.result
            db.createObjectStore(storeName, { keyPath: "id" })
        }
    }

    async set(value) {
        let request = new Promise((resolve) => {
            let dbOpenRequest = indexedDB.open(this.#databaseName, this.#version)

            const storeName = this.#storeName
            dbOpenRequest.onsuccess = function ()
            {
                let transaction = dbOpenRequest.result.transaction(storeName, "readwrite")
                let collection = transaction.objectStore(storeName)
                let result = collection.put(value)
                result.onsuccess = function (e)
                {
                    resolve()
                }
            }
        })
        
        return await request
    }
    
    async get(id) {
        let request = new Promise((resolve) =>
        {
            let dbOpenRequest = indexedDB.open(this.#databaseName, this.#version)

            const storeName = this.#storeName
            dbOpenRequest.onsuccess = function ()
            {
                let transaction = dbOpenRequest.result.transaction(storeName, "readonly")
                let collection = transaction.objectStore(storeName)
                let result = collection.get(id)

                result.onsuccess = function (e)
                {
                    resolve(result.result)
                }
            }
        })

        return await request
    }
    
    async getAllKeys() {
        let request = new Promise((resolve) =>
        {
            let dbOpenRequest = indexedDB.open(this.#databaseName, this.#version)

            const storeName = this.#storeName
            dbOpenRequest.onsuccess = function ()
            {
                let transaction = dbOpenRequest.result.transaction(storeName, "readonly")
                let collection = transaction.objectStore(storeName)
                let result = collection.getAllKeys()

                result.onsuccess = function (e)
                {
                    resolve(result.result)
                }
            }
        })

        return await request
    }
    
    delete(id) {
        let dbOpenRequest = indexedDB.open(this.#databaseName, this.#version)
        
        const storeName = this.#storeName
        dbOpenRequest.onsuccess = function ()
        {
            let transaction = dbOpenRequest.result.transaction(storeName, "readwrite")
            let collection = transaction.objectStore(storeName)
            collection.delete(id)
        }
    }
    
    async getJson(id) {
        let resource = await this.get(id)

        if (resource) {
            return JSON.stringify(resource.value)
        }
        else
        {
            return null
        }
    }
    
    async setJson(id, value) {
        await this.set({id: id, value: JSON.parse(value)});
    }
    
}
