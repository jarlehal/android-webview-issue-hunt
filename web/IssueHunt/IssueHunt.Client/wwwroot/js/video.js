import {LaudIndexedDb} from "./indexeddb.js";

export function createLaudVideo(databaseName, storeName, version) {
    return new LaudVideo(databaseName, storeName, version);
}

export class LaudVideo  {
    #databaseName
    #storeName
    #version
    
    #db
    
    constructor(databaseName, storeName, version) {
        this.#databaseName = databaseName
        this.#storeName = storeName
        this.#version = version
    }

    initialize() {
        this.#db = new LaudIndexedDb(this.#databaseName, this.#storeName, this.#version)
        this.#db.initialize()
    }
    
    async downloadIfNotAlreadyInStore(url) {
        let resource = await this.#db.get(url)

        if (!resource) {
            try {
                const req = new Request(url)
                const resp = await fetch(req)
                const blob = await resp.blob()
                await this.#db.set({id: url, blob: blob})
            } catch (e) {
                throw Error("failed to download " + e)
            }
        }
    }
    
    async getDownloadedUrls() {
        let urls = await this.#db.getAllKeys()
        return JSON.stringify(urls)
    }

    deleteVideo(id) {
        this.#db.delete(id)
    }
    
    getVideoDuration(elementId) {
        const videoElement = document.getElementById(elementId)
        return !isNaN(videoElement.duration) && isFinite(videoElement.duration) ? videoElement.duration : 0.0
    }
    
    async getIdbVideoDuration(url) {
        let resource = await this.#db.get(url)
        
        return await new Promise((resolve, reject) => {
            if (!resource) reject(new Error('Video not found'))
            
            const video = document.createElement('video')
            video.src = URL.createObjectURL(resource.blob)
            video.preload = 'metadata'
            video.style.display = 'none';

            video.addEventListener('loadedmetadata', () => {
                resolve(!isNaN(video.duration) && isFinite(video.duration) ? video.duration : 0.0)
            }, { once: true })

            video.addEventListener('error', () => {
                reject(new Error('Failed to load video metadata'))
            }, { once: true })
        });
    }

    async generateIdbThumbnail(url, seekTo = 2.0) {
        let thumbnailUrl = url + '-thumbnail'
        let resource = await this.#db.get(url)
        let thumbnail = await this.#db.get(thumbnailUrl)
        
        return await new Promise((resolve, reject) => {
            if (!resource) reject(new Error('Video not found'))
            if (thumbnail) resolve(thumbnailUrl)
            
            let video = document.createElement('video');
            video.src = URL.createObjectURL(resource.blob);
            video.preload = 'metadata';
            video.muted = true;
            video.playsInline = true;
            video.autoplay = true;
            video.crossOrigin = 'anonymous';
            
            video.addEventListener('loadeddata', () => {
                video.currentTime = seekTo;
            }, { once: true });

            video.addEventListener('seeked', async() => {
                
                await video.play();
                
                const canvas = document.createElement('canvas');
                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;

                const ctx = canvas.getContext('2d');
                ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

                canvas.toBlob(async blob => {
                    if (blob) {
                        await this.#db.set({id: thumbnailUrl, blob: blob})
                        resolve(thumbnailUrl);
                    } else {
                        reject(new Error('Thumbnail generation failed'));
                    }
                }, 'image/png');
            }, { once: true });

            video.addEventListener('error', () => {
                reject(new Error('Error loading video'));
            }, { once: true });
        });
    }
    
    async setThumbnailSource(element, url) {
        let resource = await this.#db.get(url + '-thumbnail')
        if (resource) {
            element.src = URL.createObjectURL(resource.blob)
        }
    }
    
    async setVideoSource(elementId, url, mute) {
        let resource = await this.#db.get(url)

        if (resource) {
            const videoElement = document.getElementById(elementId)
            videoElement.src = URL.createObjectURL(resource.blob)
            if (mute) videoElement.muted = "muted"
        }
    }
    
    async playVideo(elementId, seek) {
        const videoElement = document.getElementById(elementId)
        if (seek > 0) videoElement.currentTime = seek
        videoElement.play()
    }
    
}