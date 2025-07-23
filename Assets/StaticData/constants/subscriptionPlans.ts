export interface SubscriptionPlan {
  id: string;
  name: {
    tr: string;
    en: string;
    fr: string;
    ar: string;
    ru: string;
  };
  description: {
    tr: string;
    en: string;
    fr: string;
    ar: string;
    ru: string;
  };
  pricePerChild: number; // USD
  features: {
    key: string;
    available: boolean;
  }[];
  discountRates: {
    childrenCount: number;
    discountPercent: number;
  }[];
  isFree: boolean;
}

export const subscriptionPlans: SubscriptionPlan[] = [
  {
    id: 'free',
    name: {
      tr: 'Ücretsiz Plan',
      en: 'Free Plan',
      fr: 'Plan Gratuit',
      ar: 'الخطة المجانية',
      ru: 'Бесплатный план',
    },
    description: {
      tr: 'Her gün 1 aktivite, sınırlı özelliklerle.',
      en: 'One activity per day with limited features.',
      fr: 'Une activité par jour avec des fonctionnalités limitées.',
      ar: 'نشاط واحد يوميًا مع ميزات محدودة.',
      ru: 'Одна активность в день с ограниченными функциями.',
    },
    pricePerChild: 0,
    features: [
      { key: 'daily_one_activity', available: true },
      { key: 'camera_support', available: false },
      { key: 'progress_tracking', available: false },
      { key: 'parent_sharing', available: false },
      { key: 'badge_rewards', available: false },
    ],
    discountRates: [],
    isFree: true,
  },
  {
    id: 'standard',
    name: {
      tr: 'Standart Plan',
      en: 'Standard Plan',
      fr: 'Plan Standard',
      ar: 'الخطة القياسية',
      ru: 'Стандартный план',
    },
    description: {
      tr: 'Tüm aktivitelere sınırsız erişim, gelişim takibi.',
      en: 'Unlimited access to all activities with progress tracking.',
      fr: 'Accès illimité à toutes les activités avec suivi de progression.',
      ar: 'وصول غير محدود لجميع الأنشطة مع تتبع التقدم.',
      ru: 'Неограниченный доступ ко всем активностям с отслеживанием прогресса.',
    },
    pricePerChild: 7.99,
    features: [
      { key: 'daily_one_activity', available: true },
      { key: 'camera_support', available: false },
      { key: 'progress_tracking', available: true },
      { key: 'parent_sharing', available: true },
      { key: 'badge_rewards', available: true },
    ],
    discountRates: [
      { childrenCount: 2, discountPercent: 15 },
      { childrenCount: 3, discountPercent: 25 },
    ],
    isFree: false,
  },
  {
    id: 'premium',
    name: {
      tr: 'Premium Plan',
      en: 'Premium Plan',
      fr: 'Plan Premium',
      ar: 'الخطة المميزة',
      ru: 'Премиум план',
    },
    description: {
      tr: 'Kamera ile aktivite kaydı ve canlı izleme dahil.',
      en: 'Includes camera recording and live viewing.',
      fr: 'Inclut l’enregistrement et la visualisation en direct.',
      ar: 'يتضمن تسجيل النشاطات بالكاميرا والمشاهدة المباشرة.',
      ru: 'Включает запись с камеры и прямой просмотр.',
    },
    pricePerChild: 12.99,
    features: [
      { key: 'daily_one_activity', available: true },
      { key: 'camera_support', available: true },
      { key: 'progress_tracking', available: true },
      { key: 'parent_sharing', available: true },
      { key: 'badge_rewards', available: true },
    ],
    discountRates: [
      { childrenCount: 2, discountPercent: 15 },
      { childrenCount: 3, discountPercent: 25 },
    ],
    isFree: false,
  },
];
