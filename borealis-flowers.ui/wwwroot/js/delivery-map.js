window.borealisDeliveryMap = {
    maps: {},

    init: function (elementId, lat, lng, dotnetRef) {
        this.destroy(elementId);

        const map = L.map(elementId).setView([lat, lng], 14);
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19,
            attribution: "&copy; OpenStreetMap"
        }).addTo(map);

        const marker = L.marker([lat, lng], { draggable: true }).addTo(map);

        const notify = (position) => {
            dotnetRef.invokeMethodAsync("OnLocationChanged", position.lat, position.lng);
        };

        marker.on("dragend", () => notify(marker.getLatLng()));
        map.on("click", (event) => {
            marker.setLatLng(event.latlng);
            notify(event.latlng);
        });

        this.maps[elementId] = { map, marker, dotnetRef };

        setTimeout(() => map.invalidateSize(), 150);
    },

    setMarker: function (elementId, lat, lng) {
        const entry = this.maps[elementId];
        if (!entry)
            return;
        entry.marker.setLatLng([lat, lng]);
        entry.map.panTo([lat, lng]);
    },

    destroy: function (elementId) {
        const entry = this.maps[elementId];
        if (!entry)
            return;
        entry.map.remove();
        delete this.maps[elementId];
    },

    reverseGeocode: async function (lat, lng) {
        try {
            const url = `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lng}&accept-language=ru`;
            const response = await fetch(url, { headers: { Accept: "application/json" } });
            if (!response.ok)
                return null;
            const data = await response.json();
            return data.display_name ?? null;
        } catch {
            return null;
        }
    }
};
