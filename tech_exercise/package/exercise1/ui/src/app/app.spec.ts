import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('[UI-1] App shell', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('shows the app name, linking home', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const brand = (fixture.nativeElement as HTMLElement).querySelector('a.brand');

    expect(brand?.textContent).toContain('Stargate');
    expect(brand?.getAttribute('href')).toBe('/');
  });

  it('has a router outlet for the pages', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('router-outlet')).not.toBeNull();
  });
});
