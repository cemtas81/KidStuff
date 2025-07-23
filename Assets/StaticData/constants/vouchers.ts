export type VoucherType = 'free_trial' | 'lifetime' | 'upgrade';

export interface Voucher {
  code: string;
  type: VoucherType;
  description: {
    tr: string;
    en: string;
    fr: string;
    ar: string;
    ru: string;
  };
  planId?: 'standard' | 'premium'; // upgrade için hedef plan
  durationInDays?: number;         // free_trial için
  createdAt: string;               // ISO string
  expiresAt?: string;              // ISO string (opsiyonel)
  maxRedemptions?: number;         // kaç kişi kullanabilir?
  usedBy: string[];                // kullanıcı ID listesi
  active: boolean;
}

export const vouchers: Voucher[] = [
  {
    code: 'FREE30',
    type: 'free_trial',
    description: {
      tr: '30 gün ücretsiz kullanım',
      en: '30 days free access',
      fr: '30 jours d’accès gratuit',
      ar: '30 يوم استخدام مجاني',
      ru: '30 дней бесплатного доступа',
    },
    durationInDays: 30,
    createdAt: new Date().toISOString(),
    expiresAt: null,
    maxRedemptions: 1000,
    usedBy: [],
    active: true,
  },
  {
    code: 'UPGRADE2PREMIUM',
    type: 'upgrade',
    description: {
      tr: 'Premium plana ücretsiz yükseltme',
      en: 'Free upgrade to Premium plan',
      fr: 'Mise à niveau gratuite vers le plan Premium',
      ar: 'ترقية مجانية إلى الخطة المميزة',
      ru: 'Бесплатное обновление до премиум-плана',
    },
    planId: 'premium',
    createdAt: new Date().toISOString(),
    maxRedemptions: 500,
    usedBy: [],
    active: true,
  },
  {
    code: 'LIFETIMEGIFT',
    type: 'lifetime',
    description: {
      tr: 'Ömür boyu ücretsiz kullanım',
      en: 'Lifetime free access',
      fr: 'Accès gratuit à vie',
      ar: 'وصول مجاني مدى الحياة',
      ru: 'Пожизненный бесплатный доступ',
    },
    createdAt: new Date().toISOString(),
    usedBy: [],
    active: true,
  },
];
