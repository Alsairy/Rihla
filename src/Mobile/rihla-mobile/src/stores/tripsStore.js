import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import AsyncStorage from '@react-native-async-storage/async-storage';

const useTripsStore = create(
  persist(
    (set, get) => ({
      trips: [],
      currentTrip: null,
      loading: false,
      error: null,
      lastUpdated: null,
      
      setTrips: (trips) => set({ 
        trips, 
        lastUpdated: new Date().toISOString(),
        error: null 
      }),
      
      addTrip: (trip) => set((state) => ({
        trips: [...state.trips, trip],
        lastUpdated: new Date().toISOString()
      })),
      
      updateTrip: (tripId, updates) => set((state) => ({
        trips: state.trips.map(trip => 
          trip.id === tripId ? { ...trip, ...updates } : trip
        ),
        currentTrip: state.currentTrip?.id === tripId 
          ? { ...state.currentTrip, ...updates } 
          : state.currentTrip,
        lastUpdated: new Date().toISOString()
      })),
      
      removeTrip: (tripId) => set((state) => ({
        trips: state.trips.filter(trip => trip.id !== tripId),
        currentTrip: state.currentTrip?.id === tripId ? null : state.currentTrip,
        lastUpdated: new Date().toISOString()
      })),
      
      setCurrentTrip: (trip) => set({ currentTrip: trip }),
      
      clearCurrentTrip: () => set({ currentTrip: null }),
      
      setLoading: (loading) => set({ loading }),
      
      setError: (error) => set({ error }),
      
      clearError: () => set({ error: null }),
      
      getTripById: (tripId) => {
        const state = get();
        return state.trips.find(trip => trip.id === tripId);
      },
      
      getTripsByStatus: (status) => {
        const state = get();
        return state.trips.filter(trip => trip.status === status);
      },
      
      getTodaysTrips: () => {
        const state = get();
        const today = new Date().toDateString();
        return state.trips.filter(trip => 
          new Date(trip.scheduledDate).toDateString() === today
        );
      },
      
      clearTrips: () => set({
        trips: [],
        currentTrip: null,
        error: null,
        lastUpdated: null
      })
    }),
    {
      name: 'trips-storage',
      storage: createJSONStorage(() => AsyncStorage),
      partialize: (state) => ({
        trips: state.trips,
        currentTrip: state.currentTrip,
        lastUpdated: state.lastUpdated
      })
    }
  )
);

export default useTripsStore;
