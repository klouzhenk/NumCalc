export const ImageHelper = {
    getCroppedImageBase64: (cropperElement) => {
        if (!cropperElement || !cropperElement.cropper) {
            console.error("Cropper instance not found on the element.");
            return null;
        }

        const canvas = cropperElement.cropper.getCroppedCanvas({
            maxWidth: 1024,
            maxHeight: 1024
        });

        if (!canvas) return null;

        const base64String = canvas.toDataURL("image/jpeg", 0.8);

        console.log("Image successfully compressed, sending to Blazor...");

        return base64String;
    }
};