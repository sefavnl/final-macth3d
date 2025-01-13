using UnityEngine;

public class DragObject : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 screenPoint;
    private float hoverHeight = 0.1f; // Nesneyi az miktarda havaya kaldırmak için yükseklik

    void OnMouseDown()
    {
        // Fareyi tıkladığında nesneyi sürüklemeye başla
        isDragging = true;

        // Nesnenin ekran koordinatını al
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);

        // Nesne ile fare arasındaki mesafeyi hesapla
        offset = transform.position - GetMouseWorldPosition();
    }

    void OnMouseUp()
    {
        // Fareyi bıraktığında sürüklemeyi sonlandır
        isDragging = false;
    }

    void Update()
    {
        if (isDragging)
        {
            // Fare ile sürükleme işlemi yaparken pozisyonu güncelle
            Vector3 targetPosition = GetMouseWorldPosition() + offset;

            // İmlecin hemen üzerine yerleştirmek için küçük bir yükseltme ekle
            targetPosition.y = transform.position.y + hoverHeight;

            // Nesneyi hedef pozisyona yumuşak şekilde yerleştir
            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.2f); // Hareket hızını optimize ettik
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        // Fare pozisyonunu ekran koordinatlarından dünya koordinatlarına çevir
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.WorldToScreenPoint(transform.position).z; // Nesnenin z pozisyonu ile aynı uzaklıkta

        // Ekran koordinatlarından dünya koordinatlarına dönüşüm
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }
}
