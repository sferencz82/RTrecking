import { Injectable } from '@angular/core';
import { Camera, CameraResultType, CameraSource, Photo } from '@capacitor/camera';
import { Capacitor } from '@capacitor/core';

@Injectable({
  providedIn: 'root'
})
export class CameraService {
  /**
   * Check if running on a native platform (iOS/Android)
   */
  isNativePlatform(): boolean {
    return Capacitor.isNativePlatform();
  }

  /**
   * Take a photo using the device camera
   */
  async takePhoto(): Promise<File | null> {
    try {
      const image = await Camera.getPhoto({
        quality: 90,
        allowEditing: false,
        resultType: CameraResultType.DataUrl,
        source: CameraSource.Camera
      });

      return this.convertPhotoToFile(image);
    } catch (error) {
      console.error('Error taking photo:', error);
      return null;
    }
  }

  /**
   * Pick a photo from the device gallery
   */
  async pickPhoto(): Promise<File | null> {
    try {
      const image = await Camera.getPhoto({
        quality: 90,
        allowEditing: false,
        resultType: CameraResultType.DataUrl,
        source: CameraSource.Photos
      });

      return this.convertPhotoToFile(image);
    } catch (error) {
      console.error('Error picking photo:', error);
      return null;
    }
  }

  /**
   * Convert Capacitor Photo to File object
   */
  private convertPhotoToFile(photo: Photo): File | null {
    if (!photo.dataUrl) {
      return null;
    }

    // Extract base64 data
    const base64Data = photo.dataUrl.split(',')[1] || photo.dataUrl;
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    
    const byteArray = new Uint8Array(byteNumbers);
    
    // Determine file extension from format
    const extension = photo.format === 'jpeg' ? 'jpg' : photo.format || 'jpg';
    const fileName = `receipt_${Date.now()}.${extension}`;
    
    // Create File object
    const blob = new Blob([byteArray], { type: `image/${photo.format || 'jpeg'}` });
    return new File([blob], fileName, { type: `image/${photo.format || 'jpeg'}` });
  }
}
